namespace Frends.MQTT.Send
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Frends.MQTT.Send.Definitions;

    /// <summary>
    /// Main class of the Task.
    /// </summary>
    public static class MQTT
    {
        /// <summary>
        /// This is Task.
        /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.MQTT.Send).
        /// </summary>
        /// <param name="input">MQTT input parameters.</param>
        /// <param name="cancellationToken">Cancellation token given by Frends.</param>
        /// <returns>Object { string Output }.</returns>
        public static async Task<Result> Send([PropertyTab] Input input, CancellationToken cancellationToken)
        {
            var mqttSender = new MqttSender(); // Create an instance of MqttSender without logger

            try
            {
                await mqttSender.SendMqttMessageAsync(input.BrokerAddress, input.BrokerPort, input.Topic, input.Message, cancellationToken);
                return new Result(true, "Message sent successfully");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error: {ex.Message}");
            }
        }
    }
}
