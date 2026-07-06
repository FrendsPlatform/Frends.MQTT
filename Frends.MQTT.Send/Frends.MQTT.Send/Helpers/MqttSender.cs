namespace Frends.MQTT.Send;

using Frends.MQTT.Send.Definitions;
using Frends.MQTT.Send.Enums;
using Frends.MQTT.Send.Helpers;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Connect to a MQTT broker, publishes a message to a given topic, then disconnects.
/// </summary>
public class MqttSender
{
    /// <summary>
    /// Method to connect to a MQTT broker, publishes a message to a given topic, then disconnects.
    /// </summary>
    /// <param name="input">MQTT publish connection options: broker address, port, topic to publish to, message content, TLS (y/n), QoS level, optional username and password, and option to allow invalid certificates.</param>
    /// <param name="options">Additional options</param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task<Result> Send(Input input, Options options, CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(input.Host, input.BrokerPort)
            .WithCleanSession();

        bool isCertificateAuth = input.AuthenticationMethod is AuthenticationMethod.ClientCertificateFromStore
            or AuthenticationMethod.ClientCertificateFromFile
            or AuthenticationMethod.ClientCertificateFromBase64String;

        if (input.UseTls12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                    o =>
                    {
                        if (o.SslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                        {
                            if (input.AllowInvalidCertificate)
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
                var certificates = input.AuthenticationMethod switch
                {
                    AuthenticationMethod.ClientCertificateFromStore =>
                        new[]
                        {
                            CertificateLoader.LoadFromStore(
                                input.CertificateThumbprint
                                    ?? throw new ArgumentException("CertificateThumbprint cannot be empty when CertificateStore is selected."),
                                StoreName.My,
                                input.CertificateStoreLocation is CertificateStoreLocation.LocalMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser),
                        },

                    AuthenticationMethod.ClientCertificateFromFile =>
                        new[]
                        {
                            CertificateLoader.LoadFromFile(
                                input.CertificateFilePath
                                    ?? throw new ArgumentException("File path is required for CertificateSource.File authentication."),
                                input.CertificateKeyFilePath,
                                input.CertificatePassword),
                        },

                    AuthenticationMethod.ClientCertificateFromBase64String =>
                        new[]
                        {
                            CertificateLoader.LoadFromBase64(
                                input.CertificateBase64String
                                    ?? throw new ArgumentException("Base64 certificate string is required for CertificateSource.String authentication."),
                                input.CertificatePassword),
                        },

                    _ => throw new NotSupportedException($"Unsupported Authentication method: {input.AuthenticationMethod}")
                };

                tlsOptions.WithClientCertificates(certificates);
            }

            mqttOptions.WithTlsOptions(tlsOptions.Build());
        }

        switch (input.AuthenticationMethod)
        {
            case AuthenticationMethod.ClientCertificateFromFile:
            case AuthenticationMethod.ClientCertificateFromStore:
            case AuthenticationMethod.ClientCertificateFromBase64String:
                if (!string.IsNullOrEmpty(input.ClientAuthenticationName))
                    mqttOptions.WithCredentials(input.ClientAuthenticationName, "placeholder");
                break;

            case AuthenticationMethod.UsernamePassword:
                if (!string.IsNullOrEmpty(input.Username) && !string.IsNullOrEmpty(input.Password))
                    mqttOptions.WithCredentials(input.Username, input.Password);
                break;
        }

        try
        {
            await mqttClient.ConnectAsync(mqttOptions.Build(), cancellationToken);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(input.Topic)
                .WithPayload(input.Message.Payload)
                .WithQualityOfServiceLevel(input.Message.QoS)
                .WithRetainFlag(input.Message.Retain);

            if (!string.IsNullOrEmpty(input.Message.CorrelationId))
                mqttMessage.WithCorrelationData(Encoding.UTF8.GetBytes(input.Message.CorrelationId));

            await mqttClient.PublishAsync(mqttMessage.Build(), cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            return new MqttSenderException("MQTT operation was canceled", ex).Handle(options);
        }
        catch (Exception ex)
        {
            return new MqttSenderException("Failed to send MQTT message", ex).Handle(options);
        }
        finally
        {
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
        }

        return new Result
        {
            Success = true,
            Data = "Message sent.",
        };
    }
}