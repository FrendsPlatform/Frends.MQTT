namespace Frends.MQTT.Connect
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Frends.MQTT.Connect.Definitions;
    using MQTTnet;
    using MQTTnet.Protocol;

    internal class MQTTConnectionCreator
    {
        public MQTTConnectionCreator()
        {
        }

        public async Task<Result> ConnectToBroker(
            Input taskInput,
            CancellationToken cancellationToken)
        {
            var factory = new MqttClientFactory();
            using var mqttClient = factory.CreateMqttClient();

            string clientID = string.Empty;
            if (string.IsNullOrEmpty(taskInput.ClientId))
                clientID = Guid.NewGuid().ToString("N");
            else clientID = taskInput.ClientId;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(taskInput.BrokerAddress, taskInput.BrokerPort)
                .WithCleanSession(false)
                .WithWillQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(65535))
                .WithClientId(clientID)
                .Build();

            var messagesList = new List<string>();

            // in the future, any incoming messages will go on the queue
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                // add  incoming message to queue
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                messagesList.Add(payload);
                return Task.CompletedTask;
            };

            MqttClientConnectResult connectionResponse = null;
            try
            {
                // try to CONNECT
                // if unsuccessful, this will throw an exception
                connectionResponse = await mqttClient.ConnectAsync(options, cancellationToken);
            }
            catch (OperationCanceledException cException)
            {
                return new Result(success: false, newlyCreatedClient: null, clientID: clientID, serverResponse: null, cException.Message, messagesList: messagesList);
            }
            catch (Exception ex)
            {
                return new Result(success: false, newlyCreatedClient: null, clientID: clientID, serverResponse: null, error: ex.Message, messagesList: messagesList);
            }

            // after connecting, immediately SUBSCRIBE
            var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                taskInput.Topic,
                MqttQualityOfServiceLevel.AtLeastOnce,
                retainAsPublished: true,
                retainHandling: MqttRetainHandling.SendAtSubscribe)
                .Build();

            try
            {
                var subscribeResponse = await mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
            }
            catch (OperationCanceledException cException)
            {
                return new Result(success: false, newlyCreatedClient: null, clientID: clientID, serverResponse: null, cException.Message, messagesList: messagesList);
            }
            catch (Exception e)
            {
                return new Result(success: false, newlyCreatedClient: null, clientID: clientID, serverResponse: null, error: e.Message, messagesList: messagesList);
            }

            // collect messages for some seconds, then dispose at the curly bracket
            // close session dirty (without sending a DISCONNECT packet, to make the broker keep session.
            await Task.Delay(taskInput.HowLongTheTaskListensForMessages * 1000, cancellationToken);
            var result = new Result(
                success: true,
                newlyCreatedClient: mqttClient,
                clientID: clientID,
                serverResponse: connectionResponse,
                error: string.Empty,
                messagesList: messagesList);

            return result;

            // because we used using when creating the client,
            // it will be disposed here and will no longer process messages or send acknowledgements/ping!
        }
    }
}
