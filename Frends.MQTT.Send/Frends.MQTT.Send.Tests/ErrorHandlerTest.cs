namespace Frends.MQTT.Send.Tests
{
    using System;
    using System.Threading;
    using Frends.MQTT.Send;
    using Frends.MQTT.Send.Definitions;
    using NUnit.Framework;

    [TestFixture]
    internal class ErrorHandlerTest
    {
        private const string CustomErrorMessage = "CustomErrorMessage";

        private static Input InvalidInput() => new Input
        {
            Host = "invalid_address",
            BrokerPort = 1883,
            Topic = "test/topic",
            Message = "Test message",
        };

        private static Options DefaultOptions() => new Options { ThrowErrorOnFailure = true };

        [Test]
        public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
        {
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await MQTT.Send(InvalidInput(), DefaultOptions(), CancellationToken.None));
            Assert.That(ex!.Message, Does.Contain("Failed to send MQTT message"));
        }

        [Test]
        public async System.Threading.Tasks.Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
        {
            var options = DefaultOptions();
            options.ThrowErrorOnFailure = false;
            var result = await MQTT.Send(InvalidInput(), options, CancellationToken.None);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Error, Is.Not.Null);
            Assert.That(result.Error!.Message, Does.Contain("Failed to send MQTT message"));
        }

        [Test]
        public void Should_Use_Custom_ErrorMessageOnFailure()
        {
            var options = DefaultOptions();
            options.ErrorMessageOnFailure = CustomErrorMessage;
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await MQTT.Send(InvalidInput(), options, CancellationToken.None));
            Assert.That(ex!.Message, Contains.Substring(CustomErrorMessage));
        }
    }
}
