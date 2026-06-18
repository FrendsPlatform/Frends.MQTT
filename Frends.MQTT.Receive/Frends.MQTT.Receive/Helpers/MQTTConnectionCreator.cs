namespace Frends.MQTT.Receive.Helpers;

using Frends.MQTT.Receive.Definitions;
using Frends.MQTT.Receive.Enums;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class MQTTConnectionCreator
{
    public async Task<Result> ConnectToBroker(
        Input taskInput,
        Options taskOptions,
        CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        string clientID = string.IsNullOrEmpty(taskInput.ClientId)
            ? Guid.NewGuid().ToString("N")
            : taskInput.ClientId;

        var qos = (MqttQualityOfServiceLevel)taskInput.QoS;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(taskInput.Host, taskInput.BrokerPort)
            .WithCleanSession(false)
            .WithWillQualityOfServiceLevel(qos)
            .WithSessionExpiryInterval(sessionExpiryInterval: uint.MaxValue)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(taskInput.ReceivingTime))
            .WithClientId(clientID);

        bool isCertificateAuth = taskInput.AuthenticationMethod is AuthenticationMethod.ClientCertificateFromStore
            or AuthenticationMethod.ClientCertificateFromFile
            or AuthenticationMethod.ClientCertificateFromBase64String;

        if (isCertificateAuth && !taskInput.UseTls12)
            throw new ArgumentException("UseTls12 must be enabled when using a client certificate authentication method.");

        if (taskInput.UseTls12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                o =>
                {
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

            if (isCertificateAuth)
            {
                var certificates = taskInput.AuthenticationMethod switch
                {
                    AuthenticationMethod.ClientCertificateFromStore =>
                        new[]
                        {
                            CertificateLoader.LoadFromStore(
                                taskInput.CertificateThumbprint
                                    ?? throw new ArgumentException("CertificateThumbprint cannot be empty when CertificateStore is selected."),
                                StoreName.My,
                                taskInput.CertificateStoreLocation is CertificateStoreLocation.LocalMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser),
                        },

                    AuthenticationMethod.ClientCertificateFromFile =>
                        new[]
                        {
                            CertificateLoader.LoadFromFile(
                                taskInput.CertificateFilePath
                                    ?? throw new ArgumentException("File path is required for CertificateSource.File authentication."),
                                taskInput.CertificateKeyFilePath,
                                taskInput.CertificatePassword),
                        },

                    AuthenticationMethod.ClientCertificateFromBase64String =>
                        new[]
                        {
                            CertificateLoader.LoadFromBase64(
                                taskInput.CertificateBase64String
                                    ?? throw new ArgumentException("Base64 certificate string is required for CertificateSource.String authentication."),
                                taskInput.CertificatePassword),
                        },

                    _ => throw new NotSupportedException($"Unsupported Authentication method: {taskInput.AuthenticationMethod}")
                };

                tlsOptions.WithClientCertificates(certificates);
            }

            options.WithTlsOptions(tlsOptions.Build());
        }

        switch (taskInput.AuthenticationMethod)
        {
            case AuthenticationMethod.ClientCertificateFromFile:
            case AuthenticationMethod.ClientCertificateFromStore:
            case AuthenticationMethod.ClientCertificateFromBase64String:
                if (!string.IsNullOrEmpty(taskInput.ClientAuthenticationName))
                    options.WithCredentials(taskInput.ClientAuthenticationName, "placeholder");
                break;

            case AuthenticationMethod.UsernamePassword:
                if (!string.IsNullOrEmpty(taskInput.Username) && !string.IsNullOrEmpty(taskInput.Password))
                    options.WithCredentials(taskInput.Username, taskInput.Password);
                break;
        }

        var messagesList = new ConcurrentQueue<MqttMessage>();

        var handler = (Func<MqttApplicationMessageReceivedEventArgs, Task>)(e =>
        {
            var message = new MqttMessage
            {
                Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload),
                Topic = e.ApplicationMessage.Topic,
                QoS = e.ApplicationMessage.QualityOfServiceLevel,
                Retain = e.ApplicationMessage.Retain,
                ContentType = e.ApplicationMessage.ContentType,
                UserProperties = e.ApplicationMessage.UserProperties?.ToDictionary(p => p.Name, p => p.Value),
                ReceivedAt = DateTime.UtcNow,
            };

            messagesList.Enqueue(message);
            return Task.CompletedTask;
        });

        mqttClient.ApplicationMessageReceivedAsync += handler;

        MqttClientConnectResult connectionResponse = null;
        string disconnectInfo = null;

        try
        {
            mqttClient.DisconnectedAsync += e =>
            {
                disconnectInfo = $"DisconnectInfo: Reason={e.Reason}, ReasonString={e.ReasonString}, Exception={e.Exception?.Message}";
                return Task.CompletedTask;
            };

            connectionResponse = await mqttClient.ConnectAsync(options.Build(), cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return ErrorHandler.Handle(cException, taskOptions);
        }
        catch (Exception ex)
        {
            return new Exception($"Error while trying to connect to MQTT broker: {ex.Message}", ex).Handle(taskOptions);
        }

        if (connectionResponse.ResultCode != MqttClientConnectResultCode.Success)
        {
            return new ArgumentException($"Broker rejected connection: {connectionResponse.ResultCode}. ReasonString: {connectionResponse.ReasonString ?? "none provided"}. {disconnectInfo ?? string.Empty}").Handle(taskOptions);
        }

        var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                taskInput.Topic,
                qualityOfServiceLevel: qos,
                retainAsPublished: true,
                retainHandling: MqttRetainHandling.SendAtSubscribe)
            .Build();

        try
        {
            await mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new Exception($"Error while trying to subscribe to MQTT topic: {cException.Message}").Handle(taskOptions);
        }
        catch (Exception e)
        {
            return new Exception($"Error while trying to subscribe to MQTT topic: {e.Message} {disconnectInfo ?? string.Empty}").Handle(taskOptions);
        }

        await Task.Delay(taskInput.ReceivingTime * 1000, cancellationToken);

        var result = new Result
        {
            Success = true,
            CurrentClientId = clientID,
            MessagesList = messagesList,
        };

        mqttClient.ApplicationMessageReceivedAsync -= handler;

        return result;
    }
}