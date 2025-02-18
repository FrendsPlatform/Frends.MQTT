namespace Frends.MQTT.Send.Definitions;

/// <summary>
/// Result class usually contains properties of the return object.
/// </summary>
public class Result
{
    internal Result(string output)
    {
        this.Output = output;
    }

    /// <summary>
    /// Contains the input repeated the specified number of times.
    /// </summary>
    /// <example>Example of the output</example>
    public string Output { get; private set; }
}
