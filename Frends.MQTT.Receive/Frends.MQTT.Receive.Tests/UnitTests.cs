namespace Frends.MQTT.Receive.Tests;

using System.Threading.Tasks;
using Frends.MQTT.Receive.Definitions;
using NUnit.Framework;
using MQTTnet;
using System.Threading;
using System;
using System.Security.Authentication;
using System.Net.Security;
using MQTTnet.Protocol;

/// <summary>
/// These unit tests require a localhost MQTT server with TLS and authentication to be running.
/// Refer to the "Run the Tests" section in the project's README for setup instructions,
/// including how to generate certificates and start the Mosquitto broker using Docker.
/// </summary>
[TestFixture]
internal class UnitTests
{
    /// <summary>
    /// This test attempts to connect to an invalid broker address.
    /// </summary>
    /// <returns> Test succeeds when the connection is refused. </returns>
    [Test]
    public async Task Send_ShouldReturnErrorResult_WhenBrokerAddressIsInvalid()
    {
        var input = new Input
        {
            BrokerAddress = "invalid_address",
            BrokerPort = 1883,
            Topic = "test/topic",
        };

        var result = await MQTT.Receive(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
    }

    /// <summary>
    /// Test attempts to connect to an incorrect broker port.
    /// </summary>
    /// <returns> Success if it returns an error. </returns>
    [Test]
    public async Task Send_ShouldReturnErrorResult_WhenBrokerPortIsInvalid()
    {
        var input = new Input
        {
            BrokerAddress = "localhost", // dockerized Mosquitto broker
            BrokerPort = 99999, // Invalid port number
            Topic = "test/topic",
        };

        var result = await MQTT.Receive(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBroker()
    {
        var input = new Input
        {
            BrokerAddress = "localhost",
            BrokerPort = 1883,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            HowLongTheTaskListensForMessages = 10,
            Username = "testuser",
            Password = "testpass",
            UseTLS12 = false,
            QoS = 2,
            AllowInvalidCertificate = true,
        };

        var subscribeResult = await MQTT.Receive(input, default);
        Assert.IsTrue(subscribeResult.Success);

        using var publisher = new MqttClientFactory().CreateMqttClient();
        await publisher.ConnectAsync(
            new MqttClientOptionsBuilder()
                .WithTcpServer(input.BrokerAddress, input.BrokerPort)
                .WithCredentials(input.Username, input.Password)
                .Build());

        for (int i = 0; i < 6; i++)
        {
            await publisher.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic(input.Topic)
                    .WithPayload($"test message {i}")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build());
        }

        var receivedMessages = await MQTT.Receive(input, default);
        Assert.IsTrue(receivedMessages.Success);
        Assert.AreEqual(6, receivedMessages.MessagesList.Count);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTls()
    {
        var input = new Input
        {
            BrokerAddress = "localhost",
            BrokerPort = 8883,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            HowLongTheTaskListensForMessages = 10,
            Username = "testuser",
            Password = "testpass",
            UseTLS12 = true,
            QoS = 2,
            AllowInvalidCertificate = true,
        };

        var subscribeResult = await MQTT.Receive(input, default);
        Assert.IsTrue(subscribeResult.Success);

        using var publisher = new MqttClientFactory().CreateMqttClient();

        var tlsOptions = new MqttClientTlsOptions
        {
            UseTls = true,
            SslProtocol = SslProtocols.Tls12,
            CertificateValidationHandler = args =>
                args.SslPolicyErrors == SslPolicyErrors.None || input.AllowInvalidCertificate,
        };

        await publisher.ConnectAsync(
            new MqttClientOptionsBuilder()
                .WithTcpServer(input.BrokerAddress, input.BrokerPort)
                .WithCredentials(input.Username, input.Password)
                .WithTlsOptions(tlsOptions)
                .Build());

        for (int i = 0; i < 6; i++)
        {
            await publisher.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic(input.Topic)
                    .WithPayload($"TLS test {i}")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build());
        }

        var finalMessages = await MQTT.Receive(input, default);
        Assert.AreEqual(6, finalMessages.MessagesList.Count, "Missing messages. Check TLS handshake and broker logs.");
    }
}
