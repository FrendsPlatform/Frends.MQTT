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
        public string BrokerAddress { get; set; }

        /// <summary>
        /// The port of the MQTT broker.
        /// </summary>
        /// <example>1883</example>
        [Required(ErrorMessage = "Broker port is required")]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
        public int BrokerPort { get; set; }

        /// <summary>
        /// The topic to publish the message to.
        /// </summary>
        /// <example>your_topic</example>
        [Required(ErrorMessage = "Topic is required")]
        public string Topic { get; set; }

        /// <summary>
        /// The message to be published.
        /// </summary>
        /// <example>your_message</example>
        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }

        /// <summary>
        /// Additional content (optional).
        /// </summary>
        /// <example>Some example of the expected value</example>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("Lorem ipsum dolor sit amet.")]
        public string Content { get; set; } = "Lorem ipsum dolor sit amet.";
    }
}
