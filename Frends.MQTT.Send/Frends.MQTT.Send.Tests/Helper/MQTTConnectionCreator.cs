namespace Frends.MQTT.Send.Tests.Helper;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Protocol;

internal class MQTTConnectionCreator
{
    public async Task<ResultReceive> ConnectToBroker(
        InputReceive taskInput,
        CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        string clientID = string.Empty;
        if (string.IsNullOrEmpty(taskInput.ClientId))
            clientID = Guid.NewGuid().ToString("N");
        else clientID = taskInput.ClientId;

        MQTTnet.Protocol.MqttQualityOfServiceLevel qos = (MqttQualityOfServiceLevel)taskInput.QoS;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(taskInput.BrokerAddress, taskInput.BrokerPort)
            .WithCleanSession(false)
            .WithWillQualityOfServiceLevel(qos)
            .WithSessionExpiryInterval(sessionExpiryInterval: uint.MaxValue)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(taskInput.HowLongTheTaskListensForMessages))
            .WithClientId(clientID);

        if (taskInput.UseTLS12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                o =>
                {
                    // how do we proceed with the certificate the server sent?
                    if (o.SslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                    {
                        if (taskInput.AllowInvalidCertificate)
                            return true;
                        else
                            throw new InvalidCredentialException(o.SslPolicyErrors.ToString());
                    }
                    else
                    {
                        return true;
                    }
                });

            tlsOptions.WithSslProtocols(SslProtocols.Tls12);

            // build TLS settings
            options.WithTlsOptions(tlsOptions.Build());
        }

        if (!string.IsNullOrEmpty(taskInput.Username) && !string.IsNullOrEmpty(taskInput.Password))
        {
            options.WithCredentials(taskInput.Username, taskInput.Password);
        }

        var messagesList = new List<string>();

        // in the future, any incoming messages will go on the list
        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            // add  incoming message to list
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            messagesList.Add(payload);
            return Task.CompletedTask;
        };

        MqttClientConnectResult connectionResponse = null;
        try
        {
            // try to CONNECT
            // if unsuccessful, this will throw an exception
            connectionResponse = await mqttClient.ConnectAsync(options.Build(), cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new ResultReceive(success: false, clientID: clientID, cException.Message, messagesList: messagesList);
        }
        catch (Exception ex)
        {
            return new ResultReceive(success: false, clientID: clientID, error: ex.Message, messagesList: messagesList);
        }

        // after connecting, immediately SUBSCRIBE
        var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
            taskInput.Topic,
            qualityOfServiceLevel: qos,
            retainAsPublished: true,
            retainHandling: MqttRetainHandling.SendAtSubscribe)
            .Build();

        try
        {
            var subscribeResponse = await mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new ResultReceive(success: false, clientID: clientID, cException.Message, messagesList: messagesList);
        }
        catch (Exception e)
        {
            return new ResultReceive(success: false, clientID: clientID, error: e.Message, messagesList: messagesList);
        }

        // collect messages for some seconds, then dispose at the curly bracket
        // close session dirty (without sending a DISCONNECT packet, to make the broker keep session.
        await Task.Delay(taskInput.HowLongTheTaskListensForMessages * 1000, cancellationToken);
        var result = new ResultReceive(
            success: true,
            clientID: clientID,
            error: string.Empty,
            messagesList: messagesList);

        return result;

        // because we used "using" when creating the client,
        // it will be disposed here and will no longer process messages or send acknowledgements/ping!
    }
}