namespace Frends.MQTT.Receive.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

/// <summary>
/// Input class usually contains parameters that are required.
/// </summary>
public class Input
{
    /// <summary>
    /// The address of the MQTT broker.
    /// </summary>
    /// <example>broker_address</example>
    [Required(ErrorMessage = "Broker address is required")]
    public string BrokerAddress { get; set; }

    /// <summary>
    /// The port of the MQTT broker.
    /// </summary>
    /// <example>1883</example>
    [Required(ErrorMessage = "Broker port is required")]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public int BrokerPort { get; set; }

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
    public string Topic { get; set; }
}