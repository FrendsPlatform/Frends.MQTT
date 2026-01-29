namespace Frends.MQTT.Send.Tests;

using Frends.MQTT.Send;
using Frends.MQTT.Send.Definitions;
using Frends.MQTT.Send.Tests.Helper;
using NUnit.Framework;
using System;
using System.IO;
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
public class MqttTaskTests
{
    /// <summary>
    /// This test attempts to connect to an invalid broker address.
    /// </summary>
    /// <returns> Test succeeds when the connection is refused. </returns>
    [Test]
    public async Task Send_ShouldReturnErrorResult_WhenHostIsInvalid()
    {
        var input = new Input
        {
            Host = "invalid_address",
            BrokerPort = 1883,
            Topic = "test/topic",
            Message = "Test message",
        };

        var result = await MQTT.Send(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error, Does.Contain("Failed to send MQTT message"));
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
            Message = "Test message FRENDS",
        };

        var result = await MQTT.Send(input, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error, Does.Contain($"port ('{input.BrokerPort}') must be less than or equal"));
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBroker()
    {
        var inputRecieve = new InputReceive
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

        var connector = new MQTTConnectionCreator();
        var subscribeResult = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);

        Assert.IsTrue(subscribeResult.Success);

        var inputSendOne = new Input
        {
            Host = "localhost",
            BrokerPort = 1883,
            Topic = "example topic",
            Message = "Test message FRENDS 1" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = false,
            Username = "testuser",
            Password = "testpass",
            QoS = QoS.ExactlyOnce,
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 1883,
            Topic = "example topic",
            Message = "Test message FRENDS 2" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = false,
            Username = "testuser",
            Password = "testpass",
            QoS = QoS.ExactlyOnce,
        };

        var sendResultOne = await MQTT.Send(inputSendOne, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, CancellationToken.None);
        Assert.IsTrue(sendResultOne.Success);
        Assert.IsTrue(sendResultTwo.Success);

        var finalMessages = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);
        Assert.AreEqual(2, finalMessages.MessagesList.Count);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTls()
    {
        var inputRecieve = new InputReceive
        {
            Host = "localhost",
            BrokerPort = 8883,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            Username = "testuser",
            Password = "testpass",
            UseTls12 = true,
            QoS = QoS.AtLeastOnce,
            AllowInvalidCertificate = true,
        };

        var connector = new MQTTConnectionCreator();
        var subscribeResult = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);

        Assert.IsTrue(subscribeResult.Success);

        var inputSendOne = new Input
        {
            Host = "localhost",
            BrokerPort = 8883,
            Topic = "example topic",
            Message = "Test message FRENDS 1" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = true,
            Username = "testuser",
            Password = "testpass",
            QoS = QoS.AtLeastOnce,
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 8883,
            Topic = "example topic",
            Message = "Test message FRENDS 2" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = true,
            Username = "testuser",
            Password = "testpass",
            QoS = QoS.AtLeastOnce,
        };

        var sendResultOne = await MQTT.Send(inputSendOne, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, CancellationToken.None);
        Assert.IsTrue(sendResultOne.Success);
        Assert.IsTrue(sendResultTwo.Success);

        var finalMessages = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);
        Assert.AreEqual(2, finalMessages.MessagesList.Count);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTlsAndCertificate()
    {
        var inputRecieve = new InputReceive
        {
            Host = "localhost",
            BrokerPort = 8884,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            UseTls12 = true,
            QoS = QoS.AtLeastOnce,
            AllowInvalidCertificate = false,
            UseClientCertificate = true,
            CertificateSource = CertificateSource.File,
            // CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.pfx"),
            // CertificatePassword = "clientpass",
            CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt"),
            CertificateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key"),
        };

        var connector = new MQTTConnectionCreator();
        var subscribeResult = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);

        Assert.IsTrue(subscribeResult.Success, "Subscribe");

        var inputSendOne = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "example topic",
            Message = "Test message FRENDS 1" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = true,
            QoS = QoS.AtLeastOnce,
            UseClientCertificate = true,
            CertificateSource = CertificateSource.File,
            // CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.pfx"),
            // CertificatePassword = "clientpass",
            CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt"),
            CertificateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key"),
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "example topic",
            Message = "Test message FRENDS 2" + DateTime.Now.ToString(),
            AllowInvalidCertificate = true,
            UseTls12 = true,
            QoS = QoS.AtLeastOnce,
            UseClientCertificate = true,
            CertificateSource = CertificateSource.File,
            // CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.pfx"),
            // CertificatePassword = "clientpass",
            CertificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt"),
            CertificateKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key"),
        };

        var sendResultOne = await MQTT.Send(inputSendOne, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, CancellationToken.None);
        Assert.IsTrue(sendResultOne.Success, "Test1");
        Assert.IsTrue(sendResultTwo.Success, "Test2");

        var finalMessages = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);
        Assert.AreEqual(2, finalMessages.MessagesList.Count);
    }
}