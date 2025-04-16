namespace Frends.MQTT.Send.Definitions;

/// <summary>
/// The result of the sending message to MQTT broker, with possible errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Whether connection to an MQTT broker was successful or not.</param>
    /// <param name="details">Returns a confirmation message on success.</param>
    /// <param name="error">Error(s) if connecting to broker failed. Returns an empty string if no errors exist.</param>
    public Result(bool success, string? details, string? error)
    {
        Success = success;
        Data = details;
        Error = error;
    }

    /// <summary>
    /// Whether connection to the MQTT broker was successful or not.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Returns a confirmation message on success.
    /// </summary>
    /// <example>Message sent</example>
    public string? Data { get; private set; }

    /// <summary>
    /// Error(s) if connecting to broker failed. Returns an empty string if no errors exist.
    /// </summary>
    /// <example>Connection refused</example>
    public string? Error { get; private set; }
}
