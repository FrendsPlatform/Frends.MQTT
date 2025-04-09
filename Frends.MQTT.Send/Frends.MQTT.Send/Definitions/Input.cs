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
        /// <example>broker_address</example>
        [Required(ErrorMessage = "Broker address is required")]
        required public string BrokerAddress { get; set; }

        /// <summary>
        /// The port of the MQTT broker.
        /// </summary>
        /// <example>1883</example>
        [Required(ErrorMessage = "Broker port is required")]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
        required public int BrokerPort { get; set; }

        /// <summary>
        /// The topic to publish the message to.
        /// </summary>
        /// <example>your_topic</example>
        [Required(ErrorMessage = "Topic is required")]
        required public string Topic { get; set; }

        /// <summary>
        /// The message to be published.
        /// </summary>
        /// <example>your_message</example>
        [Required(ErrorMessage = "Message is required")]
        required public string Message { get; set; }

        /// <summary>
        /// Whether to use TLS authentication
        /// </summary>
        public bool UseTLS12 { get; set; }

        /// <summary>
        /// The service level for this session: 0 = At Most Once, 1 = At Least Once, 2 = Exactly Once
        /// </summary>
        [Range(0, 2, ErrorMessage = "QoS must be between 0 and 2")]
        public int QoS { get; set; } = 0;

        /// <summary>
        /// Username for authentication (NOT session name)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password for the user for authenticated connection.
        /// </summary>
        /// <example>Password123</example>
        [PasswordPropertyText]
        public string? Password { get; set; }

        /// <summary>
        /// Do not throw an exception on certificate error, and allow connection.
        /// </summary>
        /// <example>true</example>
        public bool AllowInvalidCertificate { get; set; } = false;
    }
}
