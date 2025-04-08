namespace Frends.MQTT.Send;
using System;

/// <summary>
/// Represents an exception that occurs during MQTT message sending.
/// </summary>
public class MqttSenderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttSenderException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MqttSenderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
