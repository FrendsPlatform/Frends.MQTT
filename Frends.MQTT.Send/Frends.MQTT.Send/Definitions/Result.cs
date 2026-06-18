namespace Frends.MQTT.Send.Definitions;

/// <summary>
/// The result of the sending message to MQTT broker, with possible errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Whether connection to the MQTT broker was successful or not.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Returns a confirmation message on success.
    /// </summary>
    /// <example>Message sent</example>
    public string Data { get; set; }

    /// <summary>
    /// Error(s) if connecting to broker failed. Returns an empty string if no errors exist.
    /// </summary>
    /// <example>Connection refused</example>
    public Error Error { get; set; }
}
