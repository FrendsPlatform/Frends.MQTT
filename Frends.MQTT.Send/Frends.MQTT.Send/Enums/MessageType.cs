namespace Frends.MQTT.Send.Enums;

/// <summary>
/// Enumeration that determines how the message is given to the Task.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// MqttMessage object with necessary properties.
    /// </summary>
    MqttMessage,

    /// <summary>
    /// Message type to allow the MqttMessage to be given as JSON string.
    /// </summary>
    Json,
}
