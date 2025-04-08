namespace Frends.MQTT.Send.Tests.Helper;

using System.ComponentModel;

/// <summary>
/// Input class contains parameters of the broker connection.
/// </summary>
public class InputReceive
{
    /// <summary>
    /// The address of the MQTT broker.
    /// </summary>
    /// <example>broker_addresos</example>
    public string BrokerAddress { get; set; }

    /// <summary>
    /// Sets the port of the MQTT broker.
    /// </summary>
    /// <example>1883</example>
    public int BrokerPort { get; set; }

    /// <summary>
    /// Specifies how many seconds the task (client) will live and process messages.
    /// </summary>
    public int HowLongTheTaskListensForMessages { get; set; }

    /// <summary>
    /// The client (session) identificator. This will allow to collect messages from an existing session
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The topic the client subscribes to when connecting.
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Whether to use TLS authentication
    /// </summary>
    public bool UseTLS12 { get; set; }

    /// <summary>
    /// The service level for this session: 0 = At Most Once, 1 = At Least Once, 2 = Exactly Once
    /// </summary>
    public int QoS { get; set; } = 0;

    /// <summary>
    /// Username for authentication (NOT session name)
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password for the user for authenticated connection
    /// </summary>
    /// <example>Password123</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// Do not throw an exception on certificate error, and allow connection.
    /// </summary>
    /// <example>true</example>
    public bool AllowInvalidCertificate { get; set; } = false;
}
