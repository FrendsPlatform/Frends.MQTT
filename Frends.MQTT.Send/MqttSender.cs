using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

public class MqttSender
{
    public MqttSender() { }

    public async Task SendMqttMessageAsync(
        string brokerAddress,
        int brokerPort,
        string topic,
        string message,
        CancellationToken cancellationToken)
    {
        var factory = new MqttFactory();
        using var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerAddress, brokerPort)
            .WithCleanSession()
            .Build();

        try
        {
            await mqttClient.ConnectAsync(options, cancellationToken);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(mqttMessage, cancellationToken);
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions());
        }
        catch (OperationCanceledException)
        {
            // Handle operation cancellation
        }
        catch (Exception ex)
        {
            throw new MqttSenderException("Failed to send MQTT message", ex);
        }
    }
}
