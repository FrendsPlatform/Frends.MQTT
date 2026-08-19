namespace Frends.MQTT.Send.Definitions;

using System;

/// <summary>
/// Error details for a failed Task result.
/// </summary>
public class Error
{
    /// <summary>
    /// Human-readable error message.
    /// </summary>
    /// <example>Failed to publish MQTT message to broker</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The exception that caused the failure.
    /// </summary>
    /// <example>ValidationException</example>
    public Exception? AdditionalInfo { get; set; }
}
