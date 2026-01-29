namespace Frends.MQTT.Receive.Definitions
{
    /// <summary>
    /// The service level for this session.
    /// </summary>
    public enum QoS
    {
        /// <summary>
        /// At most once
        /// </summary>
        AtMostOnce = 0,

        /// <summary>
        /// At least once
        /// </summary>
        AtLeastOnce = 1,

        /// <summary>
        /// Exactly once
        /// </summary>
        ExactlyOnce = 2,
    }

    /// <summary>
    /// The options for authenticating with certificate.
    /// </summary>
    public enum CertificateSource
    {
        /// <summary>
        /// Certificate will be fetched from the CertificateStore.
        /// </summary>
        CertificateStore,

        /// <summary>
        /// Certificate will be read from a file.
        /// </summary>
        File,

        /// <summary>
        /// String input will be used as the certificate.
        /// </summary>
        String,
    }

    /// <summary>
    /// Certificate store location.
    /// </summary>
    public enum CertificateStoreLocation
    {
        /// <summary>
        /// The X.509 certificate store assigned to the current user.
        /// </summary>
        CurrentUser,

        /// <summary>
        /// The X.509 certificate store assigned to the local machine.
        /// </summary>
        LocalMachine,
    }
}
