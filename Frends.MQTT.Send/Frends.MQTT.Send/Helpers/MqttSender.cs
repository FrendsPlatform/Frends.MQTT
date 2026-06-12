namespace Frends.MQTT.Send;

using Frends.MQTT.Send.Definitions;
using Frends.MQTT.Send.Helpers;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Send(Input input, CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(input.Host, input.BrokerPort)
            .WithCleanSession();

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

            if (input.UseClientCertificate)
            {
                IEnumerable<X509Certificate2> certificates = Array.Empty<X509Certificate2>();

                switch (input.CertificateSource)
                {
                    case CertificateSource.CertificateStore:
                        if (input.CertificateThumbprint == null)
                            throw new ArgumentException("CertificateThumbprint cannot be empty when CertificateStore is selected.");

                        certificates = new[]
                        {
                            CertificateLoader.LoadFromStore(
                            input.CertificateThumbprint,
                            StoreName.My,
                            input.CertificateStoreLocation is CertificateStoreLocation.LocalMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser),
                        };
                        break;

                    case CertificateSource.File:
                        if (input.CertificateFilePath == null)
                            throw new ArgumentException("File path is required for CertificateSource.File authentication.");

                        certificates = new[]
                        {
                            CertificateLoader.LoadFromFile(
                            input.CertificateFilePath,
                            input.CertificateKeyFilePath,
                            input.CertificatePassword),
                        };
                        break;

                    case CertificateSource.String:
                        if (input.CertificateBase64String == null)
                            throw new ArgumentException("Base64 certificate string is required for CertificateSource.String authentication.");

                        certificates = new[]
                        {
                            CertificateLoader.LoadFromBase64(
                                input.CertificateBase64String,
                                input.CertificatePassword),
                        };
                        break;

                    default:
                        break;
                }

                tlsOptions.WithClientCertificates(certificates);
            }

            options.WithTlsOptions(tlsOptions.Build());
        }

        if (!string.IsNullOrEmpty(input.Username) && !string.IsNullOrEmpty(input.Password))
            options.WithCredentials(input.Username, input.Password);

        try
        {
            await mqttClient.ConnectAsync(options.Build(), cancellationToken);

            var qos = (MqttQualityOfServiceLevel)input.QoS;

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(input.Topic)
                .WithPayload(input.Message)
                .WithQualityOfServiceLevel(qos)
                .Build();

            await mqttClient.PublishAsync(mqttMessage, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            throw new MqttSenderException("MQTT operation was canceled", ex);
        }
        catch (Exception ex)
        {
            throw new MqttSenderException("Failed to send MQTT message", ex);
        }
        finally
        {
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
        }
    }
}