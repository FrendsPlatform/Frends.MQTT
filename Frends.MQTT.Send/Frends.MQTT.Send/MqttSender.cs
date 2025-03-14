using System.Security.Authentication;
using Frends.MQTT.Send.Definitions;
using MQTTnet;
using MQTTnet.Protocol;
public class MqttSender
{
    public async Task Send(Input input, CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(input.BrokerAddress, input.BrokerPort)
            .WithCleanSession();

        MQTTnet.Protocol.MqttQualityOfServiceLevel qos = (MqttQualityOfServiceLevel)input.QoS;

        if (input.UseTLS12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                o =>
                {
                    // how do we proceed with the certificate the server sent?
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

            // build TLS settings            
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