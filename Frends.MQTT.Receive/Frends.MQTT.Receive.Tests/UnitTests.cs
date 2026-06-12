namespace Frends.MQTT.Receive.Tests;

using Frends.MQTT.Receive.Definitions;
using Frends.MQTT.Receive.Helpers;
using MQTTnet;
using MQTTnet.Protocol;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

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

    [Test]
    public async Task Send_ShouldReturnErrorResult_WhenBrokerPortIsInvalid()
    {
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 99999,
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

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTlsAndCertificate()
    {
        var certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt");
        var keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key");

        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            UseTls12 = true,
            QoS = QoS.ExactlyOnce,
            AllowInvalidCertificate = true,
            UseClientCertificate = true,
            CertificateSource = CertificateSource.File,
            CertificateFilePath = certPath,
            CertificateKeyFilePath = keyPath,
        };

        var subscribeResult = await MQTT.Receive(input, default);
        Console.WriteLine(subscribeResult.Error);
        Assert.IsTrue(subscribeResult.Success);

        using var publisher = new MqttClientFactory().CreateMqttClient();

        var tlsOptions = new MqttClientTlsOptionsBuilder()
            .WithCertificateValidationHandler(o =>
            {
                if (o.SslPolicyErrors != SslPolicyErrors.None)
                {
                    if (input.AllowInvalidCertificate)
                        return true;
                    else
                        throw new InvalidCredentialException(o.SslPolicyErrors.ToString());
                }

                return true;
            })
            .WithSslProtocols(SslProtocols.Tls12)
            .WithClientCertificates(new[] { CertificateLoader.LoadFromFile(certPath, keyPath) })
            .Build();

        await publisher.ConnectAsync(
            new MqttClientOptionsBuilder()
                .WithTcpServer(input.Host, input.BrokerPort)
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