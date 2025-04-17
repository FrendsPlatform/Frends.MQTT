namespace Frends.MQTT.Receive.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

/// <summary>
/// Input class contains parameters of the broker connection.
/// </summary>
public class Input
{
    /// <summary>
    /// The address of the MQTT broker.
    /// </summary>
    /// <example>broker_host</example>
    required public string Host { get; set; }

    /// <summary>
    /// The port of the MQTT broker.
    /// </summary>
    /// <example>1883</example>
    required public int BrokerPort { get; set; }

    /// <summary>
    /// Specifies how many seconds the task (client) will live and process messages.
    /// </summary>
    /// <example>10</example>
    public int ReceivingTime { get; set; }

    /// <summary>
    /// The client (session) identificator. This will allow to collect messages from an existing session
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public string ClientId { get; set; }

    /// <summary>
    /// The topic the client subscribes to when connecting.
    /// </summary>
    /// <example>example topic</example>
    public string Topic { get; set; }

    /// <summary>
    /// Whether to use TLS authentication.
    /// </summary>
    /// <example>false</example>
    public bool UseTls12 { get; set; }

    /// <summary>
    /// The Quality of Service (QoS) level for the MQTT session.
    /// </summary>
    /// <example>QoS.AtMostOnce</example>
    public QoS QoS { get; set; }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    /// <example>testuser</example>
    public string Username { get; set; }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    /// <example>Password123</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// When true, allows connections even if the server's TLS certificate is invalid (e.g., self-signed,
    /// expired, or hostname mismatch). WARNING: This reduces security and should only be used in
    /// development/testing environments or when connecting to internal servers with self-signed certificates.
    /// </summary>
    /// <example>true</example>
    public bool AllowInvalidCertificate { get; set; } = false;
}