

namespace Frends.MQTT.SendTask.Tests

{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Frends.MQTT.Send.Definitions;
    using Frends.MQTT.Send;
    using NUnit.Framework;

    [TestFixture]
    public class MqttTaskTests
        {
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

            var result = await MqttTask.SendMessageAsync(input, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Details.Contains("Failed"));
        }

        [Test]
        public async Task Send_ShouldReturnErrorResult_WhenBrokerPortIsInvalid()
        {
            var input = new Input
            {
                BrokerAddress = "invalid_adress", 
                BrokerPort = 99999, // Invalid port number
                Topic = "test/topic",
                Message = "Test message",
            };

            var result = await MqttTask.SendMessageAsync(input, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Details.Contains("Failed"));
        }
    }
}
