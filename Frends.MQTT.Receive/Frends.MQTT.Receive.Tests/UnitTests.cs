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
/// Start broker with auto-generated TLS/auth:
/// docker-compose up -d
/// Run tests:
/// dotnet test
/// Clean up
/// docker-compose down --volumes
/// </summary>
[TestFixture]
internal class UnitTests
{
    /// <summary>
    /// This test attempts to connect to an invalid broker address.
    /// </summary>
    /// <returns> Test succeeds when the connection is refused. </returns>
    [Test]
    public async Task Send_ShouldReturnErrorResult_WhenHostAddressIsInvalid()
    {
        var input = new Input
        {
            Host = "invalid_address",
            BrokerPort = 1883,
            Topic = "test/topic",
        };

        var result = await MQTT.Receive(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error, Does.Contain("Error while connecting host"));
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
            Host = "localhost", // dockerized Mosquitto broker
            BrokerPort = 99999, // Invalid port number
            Topic = "test/topic",
        };

        var result = await MQTT.Receive(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error, Does.Contain($"port ('{input.BrokerPort}') must be less than or equal"));
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBroker()
    {
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 1883,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            Username = "testuser",
            Password = "testpass",
            UseTls12 = false,
            QoS = QoS.ExactlyOnce,
            AllowInvalidCertificate = true,
        };

        var subscribeResult = await MQTT.Receive(input, default);
        Assert.IsTrue(subscribeResult.Success);

        using var publisher = new MqttClientFactory().CreateMqttClient();
        await publisher.ConnectAsync(
            new MqttClientOptionsBuilder()
                .WithTcpServer(input.Host, input.BrokerPort)
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
            Host = "localhost",
            BrokerPort = 8883,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            Username = "testuser",
            Password = "testpass",
            UseTls12 = true,
            QoS = QoS.ExactlyOnce,
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
                .WithTcpServer(input.Host, input.BrokerPort)
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
