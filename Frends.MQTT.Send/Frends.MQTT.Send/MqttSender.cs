using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Frends.MQTT.Send;
using Frends.MQTT.Send.Definitions;
using MQTTnet;
using MQTTnet.Protocol;

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
            .WithTcpServer(input.BrokerAddress, input.BrokerPort)
            .WithCleanSession();

        MqttQualityOfServiceLevel qos = (MqttQualityOfServiceLevel)input.QoS;

        if (input.UseTLS12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                o =>
                {
                    if (o.SslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                    {
                        if (input.AllowInvalidCertificate)
                        {
                            return true;
                        }
                        else
                        {
                            throw new InvalidCredentialException(o.SslPolicyErrors.ToString());
                        }
                    }
                    else
                    {
                        return true;
                    }
                });

            tlsOptions.WithSslProtocols(SslProtocols.Tls12);
            options.WithTlsOptions(tlsOptions.Build());
        }

        if (!string.IsNullOrEmpty(input.Username) && !string.IsNullOrEmpty(input.Password))
        {
            options.WithCredentials(input.Username, input.Password);
        }

        try
        {
            await mqttClient.ConnectAsync(options.Build(), cancellationToken);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(input.Topic)
                .WithPayload(input.Message)
                .WithQualityOfServiceLevel(qos)
                .Build();

            await mqttClient.PublishAsync(mqttMessage, cancellationToken);
        }
        catch (OperationCanceledException)
        {
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