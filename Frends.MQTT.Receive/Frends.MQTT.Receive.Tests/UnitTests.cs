namespace Frends.MQTT.Receive.Tests;

using Frends.MQTT.Receive.Definitions;
using MQTTnet;
using MQTTnet.Protocol;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTlsAndCertificate()
    {
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
            CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt"),
            CertificateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key"),
        };

        var subscribeResult = await MQTT.Receive(input, default);
        Assert.IsTrue(subscribeResult.Success);

        using var publisher = new MqttClientFactory().CreateMqttClient();

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

        var certificates = LoadCertificateFromFile(input.CertificateFilePath, input.CertificateKeyFilePath, string.Empty);
        tlsOptions.WithClientCertificates(certificates);

        await publisher.ConnectAsync(
            new MqttClientOptionsBuilder()
                .WithTcpServer(input.Host, input.BrokerPort)
                .WithTlsOptions(tlsOptions.Build())
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

    private static IEnumerable<X509Certificate2> LoadCertificateFromFile(string certificateFilePath, string? certificateKeyFilePath = null, string? password = null)
    {
        if (!File.Exists(certificateFilePath))
            throw new FileNotFoundException("Certificate file not found.", certificateFilePath);

        var extension = Path.GetExtension(certificateFilePath).ToLowerInvariant();

        if (extension is ".pfx" or ".p12")
        {
            // PFX/PKCS#12 file
            var cert = new X509Certificate2(
                certificateFilePath,
                password ?? string.Empty,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            if (!cert.HasPrivateKey)
                throw new InvalidCredentialException("The PFX certificate does not contain a private key.");

            return new[] { cert };
        }
        else if (extension is ".crt" or ".pem")
        {
            // PEM certificate. Must have key file
            if (string.IsNullOrEmpty(certificateKeyFilePath))
                throw new ArgumentException("Private key file path is required for PEM certificates.");

            if (!File.Exists(certificateKeyFilePath))
                throw new FileNotFoundException("Private key file not found.", certificateKeyFilePath);

            var cert = X509Certificate2.CreateFromPemFile(certificateFilePath, certificateKeyFilePath);

            if (!cert.HasPrivateKey)
                throw new InvalidCredentialException("The PEM certificate or key is invalid or missing the private key.");

            return new[] { cert };
        }
        else
        {
            throw new NotSupportedException($"Unsupported certificate file extension: {extension}");
        }
    }
}
