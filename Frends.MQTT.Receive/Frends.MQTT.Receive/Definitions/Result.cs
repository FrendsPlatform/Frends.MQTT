namespace Frends.MQTT.Receive.Definitions;
using System.Collections.Generic;
using MQTTnet;

/// <summary>
/// The result of the connection, including the newly created client, errors, server response, and message list.
/// </summary>
public class Result
{
    internal Result(
        bool success,
        string clientID,
        string error,
        List<string> messagesList)
    {
        this.Success = success;
        this.CurrentClientId = clientID;
        this.MessagesList = messagesList;
        this.Error = error;
    }

    /// <summary>
    /// Whether connection to an MQTT broker was successful or not.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Error (s) if connecting to broker failed
    /// </summary>
    public string Error { get; private set; }

    /// <summary>
    /// The list all incoming messages from the broker in this session during this task will be written to.
    /// </summary>
    public List<string> MessagesList { get; private set; }

    /// <summary>
    /// The MQTT client ID. Used to restore a previously opened session to receive buffered messages.
    /// When making a new connection, pass it to receive buffered messages.
    /// </summary>
    public string CurrentClientId { get; private set; }
}
