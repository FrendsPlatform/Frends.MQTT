namespace Frends.MQTT.Send.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Frends.MQTT.Send;
    using Frends.MQTT.Send.Definitions;
    using NUnit.Framework;

    /// <summary>
    /// These unit tests check if the Send method fails with incorrect parameters and succeeds with correct ones.
    /// </summary>
    [TestFixture]
    public class MqttTaskTests
    {
        private readonly string brokerAddress = Environment.GetEnvironmentVariable("MQTT_publicBrokerAddress");

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
                BrokerAddress = this.brokerAddress,
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
                BrokerAddress = this.brokerAddress,
                BrokerPort = 1883, // Invalid port number
                Topic = "test/topic",
                Message = "Test message FRENDS",
            };

            var result = await MQTT.SendMessageAsync(input, CancellationToken.None);

            Assert.IsTrue(result.Success);
        }
    }
}
