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
        /// When true, allows connections even if the server's TLS certificate is invalid (e.g., self-signed,
        /// expired, or hostname mismatch). WARNING: This reduces security and should only be used in
        /// development/testing environments or when connecting to internal servers with self-signed certificates.
        /// </summary>
        /// <example>true</example>
        [DefaultValue(false)]
        public bool AllowInvalidCertificate { get; set; }
    }
}
