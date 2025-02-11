namespace Frends.MQTT.Send.Definitions
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Input class usually contains parameters that are required.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// The address of the MQTT broker.
        /// </summary>
        /// <example>broker_address</example>
        [DefaultValue("broker_address")]
        public string BrokerAddress { get; set; } = "broker_address";

        /// <summary>
        /// The port of the MQTT broker.
        /// </summary>
        /// <example>1883</example>
        [DefaultValue(1883)]
        public int BrokerPort { get; set; } = 1883;

        /// <summary>
        /// The topic to publish the message to.
        /// </summary>
        /// <example>your_topic</example>
        [DefaultValue("your_topic")]
        public string Topic { get; set; } = "your_topic";

        /// <summary>
        /// The message to be published.
        /// </summary>
        /// <example>your_message</example>
        [DefaultValue("your_message")]
        public string Message { get; set; } = "your_message";

        /// <summary>
        /// Something that will be repeated.
        /// </summary>
        /// <example>Some example of the expected value</example>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("Lorem ipsum dolor sit amet.")]
        public string Content { get; set; } = "Lorem ipsum dolor sit amet.";
    }
}
