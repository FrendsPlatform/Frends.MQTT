namespace Frends.MQTT.Send.Tests.Helper;

using System.Collections.Concurrent;
using System.Collections.Generic;

public class ResultReceive
{
    internal ResultReceive(
        bool success,
        string clientID,
        string error,
        ConcurrentQueue<string> messagesList)
    {
        this.Success = success;
        this.CurrentClientId = clientID;
        this.MessagesList = messagesList;
        this.Error = error;
    }

    /// <summary>
    /// Whether connection to the MQTT broker was successful or not.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Error(s) if connecting to broker failed. Returns an empty string if no errors exist.
    /// </summary>
    /// <example>Connection refused</example>
    public string Error { get; private set; }

    /// <summary>
    /// All messages received from the broker in this session.
    /// </summary>
    /// <example>["message1", "message2"]</example>
    public ConcurrentQueue<string> MessagesList { get; private set; }

    /// <summary>
    /// The MQTT client ID. Used to restore a previously opened session to receive buffered messages.
    /// When making a new connection, pass it to receive buffered messages.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string CurrentClientId { get; private set; }
}
