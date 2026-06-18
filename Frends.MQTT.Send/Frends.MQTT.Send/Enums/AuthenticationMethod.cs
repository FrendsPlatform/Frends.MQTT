namespace Frends.MQTT.Send.Enums;

/// <summary>
/// Authenticaetion methods.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// Username and password are used to authenticate to the host.
    /// </summary>
    UsernamePassword,

    /// <summary>
    /// Client certificate is used to authenticate to the host.
    /// </summary>
    ClientCertificateFromStore,

    /// <summary>
    /// Certificate will be read from a file.
    /// </summary>
    ClientCertificateFromFile,

    /// <summary>
    /// String input will be used as the certificate.
    /// </summary>
    ClientCertificateFromBase64String,
}
