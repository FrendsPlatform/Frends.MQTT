namespace Frends.MQTT.Connect.Tests;

using System.Threading.Tasks;
using Frends.MQTT.Connect.Definitions;
using NUnit.Framework;

[TestFixture]
internal class UnitTests
{
    [Test]
    public async Task ShouldFailWhenConnectingToIncorrectAddress()
    {
        var input = new Input
        {
            BrokerAddress = "invalid_address",
            BrokerPort = 1883,
            ClientId = "f86c1a910f1940979fadeaf785d6b474",
            Topic = "example",
            HowLongTheTaskListensForMessages = 15,
        };

        var ret = await MQTT.Connect(input, default);

        Assert.That(ret.Success, Is.False);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToPublicBroker()
    {
        var input = new Input
        {
            BrokerAddress = @"broker.hivemq.com", //free public MQTT broker
            BrokerPort = 1883, // valid port number
            ClientId = string.Empty, // starts a new session
            Topic = "example topic",
            HowLongTheTaskListensForMessages = 10,
        };

        var result = await MQTT.Connect(input, default);

        Assert.IsTrue(result.Success);
    }
}
