using MQTTnet;
using MQTTnet.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

public static class MqttSender
{
    public static async Task SendMqttMessageAsync(
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
            Console.WriteLine($"Connected to MQTT broker at {brokerAddress}:{brokerPort}");

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await mqttClient.PublishAsync(mqttMessage, cancellationToken);
            Console.WriteLine($"Message published to topic '{topic}': {message}");

            
            await mqttClient.DisconnectAsync(new MqttClientDisconnectOptions());
            Console.WriteLine("Disconnected from MQTT broker");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
