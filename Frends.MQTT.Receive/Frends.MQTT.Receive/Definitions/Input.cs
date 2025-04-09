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
    /// <example>broker_address</example>
    [Required(ErrorMessage = "Broker address is required")]
    required public string BrokerAddress { get; set; }

    /// <summary>
    /// The port of the MQTT broker.
    /// </summary>
    /// <example>1883</example>
    [Required(ErrorMessage = "Broker port is required")]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    required public int BrokerPort { get; set; }

    /// <summary>
    /// Specifies how many seconds the task (client) will live and process messages.
    /// </summary>
    [Required(ErrorMessage = "Keep alive value is required")]
    [Range(1, 65535, ErrorMessage = "Lifetime must be between 1 and 65535")]
    public int HowLongTheTaskListensForMessages { get; set; }

    /// <summary>
    /// The client (session) identificator. This will allow to collect messages from an existing session
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The topic the client subscribes to when connecting.
    /// </summary>
    [Required(ErrorMessage = "Required: In this version of the task, this is the only opportunity to subscribe")]
    public string Topic { get; set; }

    /// <summary>
    /// Whether to use TLS authentication.
    /// </summary>
    public bool UseTLS12 { get; set; }

    /// <summary>
    /// The service level for this session: 0 = At Most Once, 1 = At Least Once, 2 = Exactly Once.
    /// </summary>
    [Range(0, 2, ErrorMessage = "QoS must be between 0 and 2")]
    public int QoS { get; set; } = 0;

    /// <summary>
    /// Username for authentication (NOT session name).
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Password for the user for authenticated connection.
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