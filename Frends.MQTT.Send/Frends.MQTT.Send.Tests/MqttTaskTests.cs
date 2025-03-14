namespace Frends.MQTT.Send.Tests
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Frends.MQTT.Send;
    using Frends.MQTT.Send.Definitions;
    using NUnit.Framework;

    /// <summary>
    /// These unit tests require a localhost MQTT server to run.
    /// To install mosquitto in docker, run:
    /// docker pull eclipse-mosquitto
    /// docker network create mosquitto_network
    /// docker run -d \
    /// --name mosquitto \
    /// --network mosquitto_network \
    /// -p 1883:1883 \
    /// -p 9001:9001 \
    /// eclipse-mosquitto
    /// Alternatively, install mosquitto locally from https://mosquitto.org/download/.
    /// </summary>
    [TestFixture]
    public class MqttTaskTests
    {
        /// <summary>
        /// This test attempts to connect to an invalid broker address.
        /// </summary>
        /// <returns> Test succeeds when the connection is refused. </returns>
        [Test]
        public async Task Send_ShouldReturnErrorResult_WhenBrokerAddressIsInvalid()
        {
            var input = new Input
            {
                BrokerAddress = "invalid_address",
                BrokerPort = 1883,
                Topic = "test/topic",
                Message = "Test message",
            };

            var result = await MQTT.SendMessageAsync(input, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Details.Contains("Failed"));
        }

        /// <summary>
        /// Test attempts to connect to an incorrect broker port.
        /// </summary>
        /// <returns> Success if it returns an error. </returns>
        [Test]
        public async Task Send_ShouldReturnErrorResult_WhenBrokerPortIsInvalid()
        {
            var input = new Input
            {
                BrokerAddress = "localhost", // dockerized Mosquitto broker
                BrokerPort = 99999, // Invalid port number
                Topic = "test/topic",
                Message = "Test message FRENDS",
            };

            var result = await MQTT.SendMessageAsync(input, CancellationToken.None);

            Assert.IsFalse(result.Success);
        }

        /// <summary>
        /// Test attempts to connect to a broker and send a message.
        /// </summary>
        /// <returns> Success if it returns an confirmation of sent message. </returns>
        [Test]
        public async Task Send_ShoulReturnSendMessageSuccess()
        {
            var input = new Input
            {
                BrokerAddress = "localhost",
                BrokerPort = 8883,
                Topic = "example topic",
                Message = "Test message FRENDS " + DateTime.Now.ToString(),
                AllowInvalidCertificate = true,
                UseTLS12 = true,
                Username = "username",
                Password = "derp",
                QoS = 1,
            };

            var result = await MQTT.SendMessageAsync(input, CancellationToken.None);

            Assert.IsTrue(result.Success);
        }
    }
}
