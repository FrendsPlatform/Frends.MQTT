namespace Frends.MQTT.Send.Definitions;

/// <summary>
/// The result of the sending message to MQTT broker, with possible errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// <see cref="Result"/> constructor.
    /// </summary>
    /// <param name="success">Whether the connection to an MQTT broker was successful or not.</param>
    /// <param name="details">A confirmation message on success.</param>
    /// <param name="error">Error(s) if connecting to the broker failed.</param>
    public Result(bool success, string? details, string? error)
    {
        Success = success;
        Data = details;
        Error = error;
    }

    /// <summary>
    /// Whether connection to an MQTT broker was successful or not.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Returns a confirmation message on success.
    /// </summary>
    public string? Data { get; private set; }

    /// <summary>
    /// Error (s) if connecting to broker failed.
    /// </summary>
    public string? Error { get; private set; }
}
