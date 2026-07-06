namespace Frends.MQTT.Send.Tests;

using Frends.MQTT.Send;
using Frends.MQTT.Send.Definitions;
using Frends.MQTT.Send.Enums;
using Frends.MQTT.Send.Tests.Helpers;
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
    private static Options options;

    [SetUp]
    public void Setup()
    {
        options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = string.Empty,
        };
    }

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
            Message = new MqttMessage
            {
                Payload = "Test message",
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
        };

        var result = await MQTT.Send(input, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.That(result.Error.Message, Does.Contain("Failed to send MQTT message"));
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
            BrokerPort = 9999, // Invalid port number
            Topic = "test/topic",
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS",
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
        };

        var result = await MQTT.Send(input, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Console.WriteLine(result.Error.Message);
        Assert.That(result.Error.Message, Does.Contain($"Failed to send MQTT message"));
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
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
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
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 1" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
            UseTls12 = false,
            Username = "testuser",
            Password = "testpass",
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 1883,
            Topic = "example topic",
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 2" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
            UseTls12 = false,
            Username = "testuser",
            Password = "testpass",
        };

        var sendResultOne = await MQTT.Send(inputSendOne, options, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, options, CancellationToken.None);
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
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
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
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 1" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            UseTls12 = true,
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
            Username = "testuser",
            Password = "testpass",
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 8883,
            Topic = "example topic",
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 2" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            UseTls12 = true,
            AuthenticationMethod = AuthenticationMethod.UsernamePassword,
            Username = "testuser",
            Password = "testpass",
        };

        var sendResultOne = await MQTT.Send(inputSendOne, options, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, options, CancellationToken.None);
        Assert.IsTrue(sendResultOne.Success);
        Assert.IsTrue(sendResultTwo.Success);

        var finalMessages = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);
        Assert.AreEqual(2, finalMessages.MessagesList.Count);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBrokerWithTlsAndPFXCertificate()
    {
        var certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.crt");
        var keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.key");
        var pfxCertPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../mosquitto/config/client.pfx");

        var inputRecieve = new InputReceive
        {
            Host = "localhost",
            BrokerPort = 8884,
            ClientId = Guid.NewGuid().ToString(),
            Topic = "example topic",
            ReceivingTime = 10,
            UseTls12 = true,
            QoS = QoS.ExactlyOnce,
            AllowInvalidCertificate = true,
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            CertificateFilePath = pfxCertPath,
            CertificatePassword = "clientpass",
            CertificateKeyFilePath = string.Empty,
        };

        var connector = new MQTTConnectionCreator();
        var subscribeResult = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);

        Console.WriteLine(subscribeResult.Error);

        Assert.IsTrue(subscribeResult.Success, "Subscribe");

        var inputSendOne = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "example topic",
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 1" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            UseTls12 = true,
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            CertificateFilePath = pfxCertPath,
            CertificatePassword = "clientpass",
            CertificateKeyFilePath = string.Empty,
        };

        var inputSendTwo = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "example topic",
            Message = new MqttMessage
            {
                Payload = "Test message FRENDS 2" + DateTime.Now.ToString(),
                QoS = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false,
                ContentType = "plain/text",
            },
            AllowInvalidCertificate = true,
            UseTls12 = true,
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            CertificateFilePath = pfxCertPath,
            CertificatePassword = "clientpass",
            CertificateKeyFilePath = string.Empty,
        };

        var sendResultOne = await MQTT.Send(inputSendOne, options, CancellationToken.None);
        var sendResultTwo = await MQTT.Send(inputSendTwo, options, CancellationToken.None);
        Assert.IsTrue(sendResultOne.Success, "Test1");
        Assert.IsTrue(sendResultTwo.Success, "Test2");

        var finalMessages = await connector.ConnectToBroker(inputRecieve, CancellationToken.None);
        Assert.AreEqual(2, finalMessages.MessagesList.Count);
    }
}