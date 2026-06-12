namespace Frends.MQTT.Receive.Helpers;

using Frends.MQTT.Receive.Definitions;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class MQTTConnectionCreator
{
    public async Task<Result> ConnectToBroker(
        Input taskInput,
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

            if (taskInput.UseClientCertificate)
            {
                var certificates = taskInput.CertificateSource switch
                {
                    CertificateSource.CertificateStore =>
                        new[]
                        {
                            CertificateLoader.LoadFromStore(
                                taskInput.CertificateThumbprint
                                    ?? throw new ArgumentException("CertificateThumbprint cannot be empty when CertificateStore is selected."),
                                System.Security.Cryptography.X509Certificates.StoreName.My,
                                taskInput.CertificateStoreLocation is CertificateStoreLocation.LocalMachine ? System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine : System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser),
                        },

                    CertificateSource.File =>
                        new[]
                        {
                            CertificateLoader.LoadFromFile(
                                taskInput.CertificateFilePath
                                    ?? throw new ArgumentException("File path is required for CertificateSource.File authentication."),
                                taskInput.CertificateKeyFilePath,
                                taskInput.CertificatePassword),
                        },

                    CertificateSource.String =>
                        new[]
                        {
                            CertificateLoader.LoadFromBase64(
                                taskInput.CertificateBase64String
                                    ?? throw new ArgumentException("Base64 certificate string is required for CertificateSource.String authentication."),
                                taskInput.CertificatePassword),
                        },

                    _ => throw new NotSupportedException($"Unsupported CertificateSource: {taskInput.CertificateSource}")
                };

                tlsOptions.WithClientCertificates(certificates);
            }

            options.WithTlsOptions(tlsOptions.Build());
        }

        if (!string.IsNullOrEmpty(taskInput.Username) && !string.IsNullOrEmpty(taskInput.Password))
            options.WithCredentials(taskInput.Username, taskInput.Password);

        var messagesList = new ConcurrentQueue<string>();

        var handler = (Func<MqttApplicationMessageReceivedEventArgs, Task>)(e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            messagesList.Enqueue(payload);
            return Task.CompletedTask;
        });

        mqttClient.ApplicationMessageReceivedAsync += handler;

        MqttClientConnectResult connectionResponse = null;
        try
        {
            connectionResponse = await mqttClient.ConnectAsync(options.Build(), cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new Result(success: false, clientID: clientID, cException.Message, messagesList: messagesList);
        }
        catch (Exception ex)
        {
            return new Result(success: false, clientID: clientID, error: $"Error while trying to connect to MQTT broker: {ex.Message}", messagesList: messagesList);
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
            return new Result(success: false, clientID: clientID, $"Error while trying to subscribe to MQTT topic: {cException.Message}", messagesList: messagesList);
        }
        catch (Exception e)
        {
            return new Result(success: false, clientID: clientID, $"Error while trying to subscribe to MQTT topic: {e.Message}", messagesList: messagesList);
        }

        await Task.Delay(taskInput.ReceivingTime * 1000, cancellationToken);

        var result = new Result(
            success: true,
            clientID: clientID,
            error: string.Empty,
            messagesList: messagesList);

        mqttClient.ApplicationMessageReceivedAsync -= handler;

        return result;
    }
}