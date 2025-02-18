namespace Frends.MQTT.SendTask.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Frends.MQTT.Send;
    using Frends.MQTT.Send.Definitions;
    using NUnit.Framework;

    /// <summary>
    /// Test class for MqttTask.
    /// </summary>
    [TestFixture]
    public class MqttTaskTests
    {
        [SetUp]
        public void Setup()
        {
            // Tutaj mo?esz doda? kod inicjalizacyjny, je?li jest potrzebny
        }

        /// <summary>
        /// Tests if the message is sent successfully.
        /// </summary>
        /// <returns>Task.</returns>
        [Test]
        public async Task SendMessageAsync_ShouldSendSuccessfully()
        {
            Console.WriteLine("Test: SendMessageAsync_ShouldSendSuccessfully START");

            var input = new Input
            {
                BrokerAddress = "test.mosquitto.org",
                BrokerPort = 1883,
                Topic = "test/topic",
                Message = "Test message",
            };

            var cancellationToken = CancellationToken.None;

            try
            {
                var result = await MqttTask.SendMessageAsync(input, cancellationToken);

                Console.WriteLine($"Test: Result -> Success: {result.Success}, Details: {result.Details}");

                Assert.IsTrue(result.Success, "Message sending failed when it should have succeeded.");
                Assert.AreEqual("Message sent successfully", result.Details, "Message details do not match expected output.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test Failed: {ex.Message}\n{ex.StackTrace}");
                Assert.Fail($"Unexpected exception occurred: {ex.Message}");
            }

            Console.WriteLine("Test: SendMessageAsync_ShouldSendSuccessfully END");
        }

    }
}
