namespace Frends.MQTT.Connect;
using System;
using System.ComponentModel;
using System.Threading;
using Frends.MQTT.Connect.Definitions;
using System.Threading.Tasks;

/// <summary>
/// Main class of the Task to connect to an MQTT broker and start a message listening session.
/// </summary>
public static class MQTT
{
    /// <summary>
    /// This is the Task to connnect to an MQTT broker and start a listening session.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.MQTT.Connect).
    /// </summary>
    /// <param name="input">MQTT broker connection options: broker address, port, duration of task, topic subscribed to, and (optional) previous session id </param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>The result of the connection, including the newly created client, errors, server response, and message list</returns>
    public static async Task<Result> ConnectAndReceive([PropertyTab] Input input, CancellationToken cancellationToken)
    {
        try
        {
            var connector = new MQTTConnectionCreator();
            var result = await connector.ConnectToBroker(input, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return new Result(success: false, null, clientID: string.Empty, null, error: ex.Message, null);
        }
    }
}
