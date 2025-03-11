using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Frends.MQTT.Send.Definitions;

namespace Frends.MQTT.Send
{
    public static class MQTT
    {
        public static async Task<Result> SendMessageAsync([PropertyTab] Input input, CancellationToken cancellationToken)
        {
            try
            {
                var mqttSender = new MqttSender();
                await mqttSender.Send(input, cancellationToken);

                return new Result(true, "Message sent");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error: {ex.Message}");
            }
        }
    }
}
