namespace Frends.MQTT.Send.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.MQTT.Send;
using Frends.MQTT.Send.Definitions;
using Frends.MQTT.Send.Enums;
using NUnit.Framework;

[TestFixture]
public class MqttSenderFailurePathTests
{
    private static Options TestOptions() => new Options
    {
        ThrowErrorOnFailure = false,
        ErrorMessageOnFailure = string.Empty,
    };

    private static Input CreateInput() => new Input
    {
        Host = "localhost",
        BrokerPort = 1883,
        AuthenticationMethod = AuthenticationMethod.UsernamePassword,
        Username = "testuser",
        Password = "testpass",
        Message = new MqttMessage
        {
            Topic = "coverage/send",
            PayloadType = PayloadType.String,
            Payload = "coverage test",
            QoS = QoS.AtLeastOnce,
        },
    };

    [Test]
    public async Task Send_ReturnsFailure_WhenUserPropertiesJsonIsMalformed()
    {
        var input = CreateInput();
        input.Message.UserProperties = "{ malformed json";

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.Message, Does.Contain("Failed to send MQTT message"));
    }

    [Test]
    public async Task Send_ReturnsFailure_WhenQosValueIsUnsupported()
    {
        var input = CreateInput();
        input.Message.QoS = (QoS)99;

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error!.Message, Does.Contain("Failed to send MQTT message"));
    }

    [Test]
    public async Task Send_ReturnsFailure_WhenCertificateFileAuthenticationHasNoPath()
    {
        var input = CreateInput();
        input.UseTls12 = true;
        input.AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile;
        input.CertificateFilePath = null;

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error!.Message, Does.Contain("File path is required"));
    }

    [Test]
    public async Task Send_ReturnsFailure_WhenBase64CertificateAuthenticationHasNoValue()
    {
        var input = CreateInput();
        input.UseTls12 = true;
        input.AuthenticationMethod = AuthenticationMethod.ClientCertificateFromBase64String;
        input.CertificateBase64String = null;

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error!.Message, Does.Contain("Base64 certificate string is required"));
    }

    [Test]
    public async Task Send_ReturnsFailure_WhenBase64CertificateIsMalformed()
    {
        var input = CreateInput();
        input.UseTls12 = true;
        input.AuthenticationMethod = AuthenticationMethod.ClientCertificateFromBase64String;
        input.CertificateBase64String = "not-base64";

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error!.Message, Does.Contain("not-base64"));
    }
    [TestCase(QoS.AtMostOnce)]
    [TestCase(QoS.AtLeastOnce)]
    [TestCase(QoS.ExactlyOnce)]
    public async Task Send_Succeeds_ForEachSupportedQos(QoS qos)
    {
        var input = CreateInput();
        input.Message.QoS = qos;

        var result = await MQTT.Send(input, TestOptions(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.EqualTo("Message sent."));
    }

}
