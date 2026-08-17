namespace Frends.MQTT.Send.Definitions;

/// <summary>
/// The result of the sending message to MQTT broker, with possible errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="success">Whether the operation completed successfully.</param>
    /// <param name="data">Returns a confirmation message on success.</param>
    /// <param name="error">Error details. Null when Success is true.</param>
    public Result(bool success, string? data = null, Error? error = null)
    {
        Success = success;
        Data = data;
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
    /// <example>Message sent.</example>
    public string? Data { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error? Error { get; private set; }
}
