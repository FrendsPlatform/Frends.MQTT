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
}
