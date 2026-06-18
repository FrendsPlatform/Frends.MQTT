namespace Frends.MQTT.Receive.Definitions;

using System.Collections.Concurrent;

/// <summary>
/// The result of the connection, including the newly created client, errors, server response, and message list.
/// </summary>
public class Result
{
    /// <summary>
    /// Whether connection to the MQTT broker was successful or not.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Error(s) if connecting to broker failed. Returns an empty string if no errors exist.
    /// </summary>
    /// <example>Connection refused</example>
    public Error Error { get; set; }

    /// <summary>
    /// All MQTT messages received from the broker during the current session.
    /// </summary>
    /// <example>
    /// [
    ///   {
    ///     "Payload": "{"temperature":22.5}",
    ///     "Topic": "sensors/office",
    ///     "QoS": "AtLeastOnce",
    ///     "Retain": false,
    ///     "ContentType": "application/json",
    ///     "UserProperties": {
    ///       "DeviceId": "sensor-001"
    ///     },
    ///     "ReceivedAt": "2026-06-18T14:30:15Z"
    ///   }
    /// ]
    /// </example>
    public ConcurrentQueue<MqttMessage> MessagesList { get; set; }

    /// <summary>
    /// The MQTT client ID. Used to restore a previously opened session to receive buffered messages.
    /// When making a new connection, pass it to receive buffered messages.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string CurrentClientId { get; set; }
}
