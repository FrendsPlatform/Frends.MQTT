namespace Frends.MQTT.Send.Definitions
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Input class for MQTT message publishing.
    /// </summary>
    public class Input
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
        /// The topic to publish the message to.
        /// </summary>
        /// <example>your_topic</example>
        required public string Topic { get; set; }

        /// <summary>
        /// The message to be published.
        /// </summary>
        /// <example>your_message</example>
        required public string Message { get; set; }

        /// <summary>
        /// Whether to use TLS authentication
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
        public string? Username { get; set; }

        /// <summary>
        /// Password for authentication.
        /// </summary>
        /// <example>Password123</example>
        [PasswordPropertyText]
        public string? Password { get; set; }

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
        public string? CertificateThumbprint { get; set; }

        /// <summary>
        /// Certification store
        /// </summary>
        /// <example>CertificateStoreLocation.CurrentUser</example>
        [UIHint(nameof(CertificateSource), "", CertificateSource.CertificateStore)]
        public CertificateStoreLocation CertificateStoreLocation { get; set; }

        /// <summary>
        /// File path to the certificate to be used in authentication.
        /// </summary>
        /// <example>C:\cert.pfx</example>
        [UIHint(nameof(CertificateSource), "", CertificateSource.File)]
        public string? CertificateFilePath { get; set; }

        /// <summary>
        /// Path to the certificate key file to be used in authentication. Needed when used PEM typed certificates.
        /// </summary>
        /// <example>C:\cert.key</example>
        [UIHint(nameof(CertificateSource), "", CertificateSource.File)]
        public string? CertificateKeyFilePath { get; set; }

        /// <summary>
        /// The certificate as Base64 string to be used in authentication.
        /// </summary>
        /// <example>C:\cert.pfx</example>
        [UIHint(nameof(CertificateSource), "", CertificateSource.String)]
        public string? CertificateBase64String { get; set; }

        /// <summary>
        /// Password for the certificate file.
        /// </summary>
        /// <example>password</example>
        [UIHint(nameof(CertificateSource), "", CertificateSource.File, CertificateSource.String)]
        [DisplayFormat(DataFormatString = "Text")]
        [PasswordPropertyText]
        public string? CertificatePassword { get; set; }

        /// <summary>
        /// When true, allows connections even if the server's TLS certificate is invalid (e.g., self-signed,
        /// expired, or hostname mismatch). WARNING: This reduces security and should only be used in
        /// development/testing environments or when connecting to internal servers with self-signed certificates.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        public bool AllowInvalidCertificate { get; set; }
    }
}
