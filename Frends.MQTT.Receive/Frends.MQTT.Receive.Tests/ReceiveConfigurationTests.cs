namespace Frends.MQTT.Receive.Tests;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.MQTT.Receive;
using Frends.MQTT.Receive.Definitions;
using Frends.MQTT.Receive.Enums;
using NUnit.Framework;

[TestFixture]
public class ReceiveConfigurationTests
{
    [Test]
    public async Task Receive_ReturnsError_WhenCertificateAuthenticationIsUsedWithoutTls()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 1883,
            Topic = "test/topic",
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            CertificateFilePath = "unused.pfx",
            UseTls12 = false,
        };

        var result = await MQTT.Receive(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("UseTls12 must be enabled"));
    }

    [Test]
    public async Task Receive_ReturnsError_WhenCertificateFilePathIsMissing()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "test/topic",
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            UseTls12 = true,
        };

        var result = await MQTT.Receive(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("File path is required"));
    }

    [Test]
    public async Task Receive_ReturnsError_WhenCertificateFileDoesNotExist()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "test/topic",
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromFile,
            CertificateFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pfx"),
            UseTls12 = true,
        };

        var result = await MQTT.Receive(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Certificate file not found"));
    }

    [Test]
    public async Task Receive_ReturnsError_WhenBase64CertificateIsMissing()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "test/topic",
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromBase64String,
            UseTls12 = true,
        };

        var result = await MQTT.Receive(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Base64 certificate string is required"));
    }

    [Test]
    public async Task Receive_ReturnsError_WhenBase64CertificateIsMalformed()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var input = new Input
        {
            Host = "localhost",
            BrokerPort = 8884,
            Topic = "test/topic",
            AuthenticationMethod = AuthenticationMethod.ClientCertificateFromBase64String,
            CertificateBase64String = "not-base64",
            UseTls12 = true,
        };

        var result = await MQTT.Receive(input, options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters."));
    }
}
