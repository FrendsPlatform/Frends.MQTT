namespace Frends.MQTT.Send.Tests;

using System;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Frends.MQTT.Send.Helpers;
using NUnit.Framework;

[TestFixture]
public class CertificateLoaderTests
{
    private const string Password = "test-password";
    private string _directory = null!;
    private string _pfxPath = null!;
    private string _pemPath = null!;
    private string _keyPath = null!;
    private string _publicPfxPath = null!;
    private string _base64 = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "Frends.MQTT.Send.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=MQTT-Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));

        var pfx = certificate.Export(X509ContentType.Pfx, Password);
        _pfxPath = Path.Combine(_directory, "client.pfx");
        File.WriteAllBytes(_pfxPath, pfx);
        _base64 = Convert.ToBase64String(pfx);

        _pemPath = Path.Combine(_directory, "client.pem");
        _keyPath = Path.Combine(_directory, "client.key");
        File.WriteAllText(_pemPath, certificate.ExportCertificatePem());
        using var privateKey = certificate.GetRSAPrivateKey()!;
        File.WriteAllText(_keyPath, privateKey.ExportPkcs8PrivateKeyPem());

        _publicPfxPath = Path.Combine(_directory, "public-only.pfx");
        using var publicOnly = new X509Certificate2(certificate.RawData);
        File.WriteAllBytes(_publicPfxPath, publicOnly.Export(X509ContentType.Pfx));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public void GetKeyStorageFlags_ContainsRequiredFlags()
    {
        var flags = CertificateLoader.GetKeyStorageFlags();

        Assert.That(flags.HasFlag(X509KeyStorageFlags.PersistKeySet), Is.True);
        Assert.That(flags.HasFlag(X509KeyStorageFlags.Exportable), Is.True);
        Assert.That(
            flags.HasFlag(X509KeyStorageFlags.UserKeySet) || flags.HasFlag(X509KeyStorageFlags.MachineKeySet),
            Is.True);
    }

    [Test]
    public void LoadFromFile_LoadsPfxWithPrivateKey()
    {
        using var certificate = CertificateLoader.LoadFromFile(_pfxPath, password: Password);

        Assert.That(certificate.HasPrivateKey, Is.True);
    }

    [Test]
    public void LoadFromFile_LoadsPemAndSeparatePrivateKey()
    {
        using var certificate = CertificateLoader.LoadFromFile(_pemPath, _keyPath);

        Assert.That(certificate.HasPrivateKey, Is.True);
    }

    [Test]
    public void LoadFromFile_ThrowsWhenCertificateFileDoesNotExist()
    {
        var path = Path.Combine(_directory, "missing.pfx");

        Assert.Throws<FileNotFoundException>(() => CertificateLoader.LoadFromFile(path));
    }

    [Test]
    public void LoadFromFile_ThrowsForUnsupportedExtension()
    {
        var path = Path.Combine(_directory, "certificate.txt");
        File.WriteAllText(path, "not a certificate");

        Assert.Throws<NotSupportedException>(() => CertificateLoader.LoadFromFile(path));
    }

    [Test]
    public void LoadFromFile_ThrowsWhenPemPrivateKeyPathIsMissing()
    {
        Assert.Throws<ArgumentException>(() => CertificateLoader.LoadFromFile(_pemPath));
    }

    [Test]
    public void LoadFromFile_ThrowsWhenPemPrivateKeyFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() =>
            CertificateLoader.LoadFromFile(_pemPath, Path.Combine(_directory, "missing.key")));
    }

    [Test]
    public void LoadFromFile_ThrowsWhenPfxHasNoPrivateKey()
    {
        Assert.Throws<InvalidCredentialException>(() => CertificateLoader.LoadFromFile(_publicPfxPath));
    }

    [Test]
    public void LoadFromBase64_LoadsCertificateWithPrivateKey()
    {
        using var certificate = CertificateLoader.LoadFromBase64(_base64, Password);

        Assert.That(certificate.HasPrivateKey, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void LoadFromBase64_ThrowsForEmptyInput(string? value)
    {
        Assert.Throws<ArgumentException>(() => CertificateLoader.LoadFromBase64(value!));
    }

    [Test]
    public void LoadFromBase64_ThrowsForInvalidBase64()
    {
        Assert.Throws<FormatException>(() => CertificateLoader.LoadFromBase64("not-base64"));
    }

    [Test]
    public void LoadFromBase64_ThrowsWhenCertificateHasNoPrivateKey()
    {
        var raw = File.ReadAllBytes(_publicPfxPath);
        var base64 = Convert.ToBase64String(raw);

        Assert.Throws<InvalidCredentialException>(() => CertificateLoader.LoadFromBase64(base64));
    }

    [Test]
    public void LoadFromStore_ThrowsWhenThumbprintDoesNotExist()
    {
        Assert.Throws<ArgumentException>(() =>
            CertificateLoader.LoadFromStore("0000000000000000000000000000000000000000", StoreName.My, StoreLocation.CurrentUser));
    }
}
