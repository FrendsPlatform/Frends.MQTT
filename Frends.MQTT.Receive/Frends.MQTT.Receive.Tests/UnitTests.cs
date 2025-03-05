namespace Frends.MQTT.Receive.Tests;

using System.Threading.Tasks;
using Frends.MQTT.Receive.Definitions;
using NUnit.Framework;
using MQTTnet;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Threading;
using System;
using System.Diagnostics;

[TestFixture]
internal class UnitTests
{
    private readonly string brokerAddress = Environment.GetEnvironmentVariable("MQTT_publicBrokerAddress");

    [Test]
    public async Task ShouldFailWhenConnectingToIncorrectAddress()
    {
        var input = new Input
        {
            BrokerAddress = "invalid_address",
            BrokerPort = 1883,
            ClientId = "f86c1a910f1940979fadeaf785d6b474",
            Topic = "example",
            HowLongTheTaskListensForMessages = 15,
        };

        var ret = await MQTT.ConnectAndReceive(input, default);

        Assert.That(ret.Success, Is.False);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToPublicBroker()
    {
        var input = new Input
        {
            BrokerAddress = brokerAddress, // free public MQTT broker
            BrokerPort = 1883, // valid port number
            ClientId = "f86c1a910f1940979fadeaf785d6b474", // starts a new session
            Topic = "example topic",
            HowLongTheTaskListensForMessages = 5,
        };

        // start a session to create an inbox
        var result = await MQTT.ConnectAndReceive(input, default);

        Assert.IsTrue(result.Success);

        if (result.Success)
        {
            // send a test message using a separate sending client (so that the sender and recipient are separate)
            var factory = new MqttClientFactory();
            using var sendingMqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(input.BrokerAddress, input.BrokerPort)
            .WithClientId("c1e06c346ef1456fb4e2b038706d9693")
            .WithCleanSession(true)
            .Build();

            // wait a bit, this broker has a connection rate limit
            await Task.Delay(10000);

            var senderClientConnectResult = await sendingMqttClient.ConnectAsync(options, default);

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(input.Topic)
                        .WithPayload("test message " + i)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();

                    var sendingResult = await sendingMqttClient.PublishAsync(mqttMessage, default);
                    Assert.IsTrue(sendingResult.IsSuccess);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("test cancelled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to send MQTT message: ", ex.Message);
            }
            finally
            {
                await sendingMqttClient.DisconnectAsync(new MqttClientDisconnectOptions());
            }

            // wait 10s for the broker to permit a new connection
            await Task.Delay(10000);

            // now collect the sent message from the inbox
            input.HowLongTheTaskListensForMessages = 10;
            var resultOfReceivingMessage = await MQTT.ConnectAndReceive(input, default);

            foreach(var message in resultOfReceivingMessage.MessagesList)
            {
                Debug.WriteLine(message);
            }

            Assert.IsTrue(resultOfReceivingMessage.MessagesList.Count > 0);
            Assert.IsTrue(resultOfReceivingMessage.Success);
        }
    }
}
