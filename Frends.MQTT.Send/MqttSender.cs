using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

public class MqttSender
{
    private readonly ILogger<MqttSender> _logger;

    public MqttSender(ILogger<MqttSender> logger)
    {
        _logger = logger;
    }

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
            _logger.LogInformation("Connected to MQTT broker at {Address}:{Port}", brokerAddress, brokerPort);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(mqttMessage, cancellationToken);
            _logger.LogInformation("Message published to topic '{Topic}': {Message}", topic, message);

            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions());
            _logger.LogInformation("Disconnected from MQTT broker");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MQTT message");
            throw new MqttSenderException("Failed to send MQTT message", ex);
        }
    }
}
