namespace Frends.MQTT.Send.Tests.Helper;

using Frends.MQTT.Send.Definitions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input class contains parameters of the broker connection.
/// </summary>
public class InputReceive
{
    /// <summary>
    /// The address of the MQTT broker.
    /// </summary>
    /// <example>broker_host</example>
    required public string Host { get; set; }

    /// <summary>
    /// The port of the MQTT broker.
    /// </summary>
    /// <example>1883</example>
    required public int BrokerPort { get; set; }

    /// <summary>
    /// Specifies how many seconds the task (client) will live and process messages.
    /// </summary>
    /// <example>10</example>
    public int ReceivingTime { get; set; }

    /// <summary>
    /// The client (session) identificator. This will allow to collect messages from an existing session
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string ClientId { get; set; }

    /// <summary>
    /// The topic the client subscribes to when connecting.
    /// </summary>
    /// <example>example topic</example>
    public string Topic { get; set; }

    /// <summary>
    /// Whether to use TLS authentication.
    /// </summary>
    /// <example>false</example>
    public bool UseTls12 { get; set; }

    /// <summary>
    /// The Quality of Service (QoS) level for the MQTT session.
    /// </summary>
    /// <example>QoS.AtMostOnce</example>
    public QoS QoS { get; set; }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    /// <example>testuser</example>
    public string Username { get; set; }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    /// <example>Password123</example>
    public string Password { get; set; }

    /// <summary>
    /// Determines whether certificate will be use in authentication.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool UseClientCertificate { get; set; }

    /// <summary>
    /// If set anything but CertificateSource .None Task will use certificate in authentication.
    /// </summary>
    /// <example>CertificateSource .Store</example>
    [UIHint(nameof(UseClientCertificate), "", true)]
    public CertificateSource CertificateSource { get; set; }

    /// <summary>
    /// Thumbprint to use to get the correct certificate from certificate store.
    /// </summary>
    /// <example>3F7A9C4D1B2E6F8890A1B2C3D4E5F6789012ABCD</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.CertificateStore)]
    [DisplayFormat(DataFormatString = "Text")]
    public string CertificateThumbprint { get; set; }

    /// <summary>
    /// Certification store
    /// </summary>
    /// <example>CertificateStoreLocation.CurrentUser</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.CertificateStore)]
    public CertificateStoreLocation CertificateStoreLocation { get; set; }

    /// <summary>
    /// Path to the certificate file to be used in authentication.
    /// </summary>
    /// <example>C:\cert.pfx</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.File)]
    public string CertificateFilePath { get; set; }

    /// <summary>
    /// Path to the certificate key file to be used in authentication. Needed when used PEM typed certificates.
    /// </summary>
    /// <example>C:\cert.key</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.File)]
    public string CertificateKeyFilePath { get; set; }

    /// <summary>
    /// The certificate as Base64 string to be used in authentication.
    /// </summary>
    /// <example>MIIJ0QIBAzCCCf8GCSqGSIb3DQEHAaCCCfAEggn0MIIC8TCCAdkGCSqGSIb3DQEH...</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.String)]
    public string CertificateBase64String { get; set; }

    /// <summary>
    /// Password for the certificate file. Needed when using PFX typed certificates.
    /// </summary>
    /// <example>password</example>
    [UIHint(nameof(CertificateSource), "", CertificateSource.File, CertificateSource.String)]
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    public string CertificatePassword { get; set; }

    /// <summary>
    /// When true, allows connections even if the server's TLS certificate is invalid (e.g., self-signed,
    /// expired, or hostname mismatch). WARNING: This reduces security and should only be used in
    /// development/testing environments or when connecting to internal servers with self-signed certificates.
    /// </summary>
    /// <example>true</example>
    public bool AllowInvalidCertificate { get; set; } = false;
}
