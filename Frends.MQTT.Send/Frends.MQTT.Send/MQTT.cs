namespace Frends.MQTT.Send;

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.MQTT.Send.Definitions;

/// <summary>
/// Main class of the Task to connect to a MQTT broker, publishes a message to a given topic, then disconnects.
/// </summary>
public static class MQTT
{
    /// <summary>
    /// This is the Task to connect to a MQTT broker, publishes a message to a given topic, then disconnects.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.MQTT.Send).
    /// </summary>
    /// <param name="input">MQTT publish connection options: broker address, port, topic to publish to, message content, TLS (y/n), QoS level, optional username and password, and option to allow invalid certificates.</param>
    /// <param name="options">Additional options</param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>Object { bool Success, string Data, string Error }</returns>
    public static async Task<Result> Send([PropertyTab] Input input, Options options, CancellationToken cancellationToken)
    {
        var mqttSender = new MqttSender();
        var result = await mqttSender.Send(input, options, cancellationToken);

        return result;
    }
}
