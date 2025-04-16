namespace Frends.MQTT.Receive;

using System;
using System.ComponentModel;
using System.Threading;
using Frends.MQTT.Receive.Definitions;
using System.Threading.Tasks;

/// <summary>
/// Main class of the Task to connect to an MQTT broker and start a message listening session.
/// </summary>
public static class MQTT
{
    /// <summary>
    /// This is the Task to connnect to an MQTT broker and start a listening session.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.MQTT.Receive).
    /// </summary>
    /// <param name="input">MQTT broker connection options: broker address, port, duration of task, topic subscribed to, (optional) previous session id, TLS (y/n), QoS </param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>Object { bool Success, string CurrentClientId, string Error, List(string) MessagesList }</returns>
    public static async Task<Result> Receive([PropertyTab] Input input, CancellationToken cancellationToken)
    {
        try
        {
            var connector = new MQTTConnectionCreator();
            var result = await connector.ConnectToBroker(input, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return new Result(success: false, null, error: ex.Message, null);
        }
    }
}
