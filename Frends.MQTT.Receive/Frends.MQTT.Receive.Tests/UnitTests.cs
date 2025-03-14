namespace Frends.MQTT.Receive.Tests;

using System.Threading.Tasks;
using Frends.MQTT.Receive.Definitions;
using NUnit.Framework;
using MQTTnet;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Threading;
using System;
using System.Diagnostics;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

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

        var ret = await MQTT.Receive(input, default);

        Assert.That(ret.Success, Is.False);
    }

    [Test]
    public async Task ShouldSuccessfullyConnectToBroker()
    {
        var testCrt = @"-----BEGIN CERTIFICATE-----
MIIDuTCCAqGgAwIBAgIUOJhb92KAQIV6qKRs2YFiWhbYR7UwDQYJKoZIhvcNAQEL
BQAwgYQxCzAJBgNVBAYTAlBMMRIwEAYDVQQIDAlQb21vcnNraWUxDzANBgNVBAcM
BkdkYW5zazENMAsGA1UECgwEaG9tZTENMAsGA1UECwwEZGVycDENMAsGA1UEAwwE
ZGVycDEjMCEGCSqGSIb3DQEJARYUamNvb3Blcng4NkBnbWFpbC5jb20wHhcNMjUw
MzEzMDk0MTI3WhcNMjYwMzEzMDk0MTI3WjCBhDELMAkGA1UEBhMCUEwxEjAQBgNV
BAgMCVBvbW9yc2tpZTEPMA0GA1UEBwwGR2RhbnNrMQ0wCwYDVQQKDARob21lMQ0w
CwYDVQQLDARkZXJwMQ0wCwYDVQQDDARkZXJwMSMwIQYJKoZIhvcNAQkBFhRqY29v
cGVyeDg2QGdtYWlsLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AKOPVvtWSqoJbYIFC+5xe2VgzqoO2fC+yWQpLog7ImK4h/Fz9CWkmDfEpSrfF1ZC
Ebd0iauaeoAjhMC/jT87LN+JL33GC/dWjmKTCL1iTvNPrMoSafPWGGE5OvC3I92m
u4nRRQb5+9IRw7g92LKRLQodbz0cn5SdyUlU961AwQFqm4PR74pB78dqa7soLCTK
qSdb/jcak2ixUCpfxPmLPDaCwBVW+0F2g0uaZWTamoArn2rBN6Zsf2EZTjAjaehB
LKRcFntRhtZzGJJynH7O/+hc3Il7ro5OAFzdxY1n0UvtbQw6gS65DP+IMgncK/dF
jlvdyM3K4TqzgUItjXyWFJkCAwEAAaMhMB8wHQYDVR0OBBYEFFWUJcOzcIJOtGSz
t1nKscLaiPHNMA0GCSqGSIb3DQEBCwUAA4IBAQCgVv/DVHAn7mRSRw57WEabaXzT
S+xr8MClnrj03Hz9jVj0YIAGr2i+PUM5juumBjHO0Ucxs5a+bzKnj9CaSh05Hwvg
unpNxX4dRWny091Whwl5DarwXBP8kNc6MQL8bSgIZxfB0dFjOHDdHaCk2NrfY552
zECTaYRvOFd9D/JV8V10vah7QWdnJdAQe83+l1HxXnaXEuH+CzRjqUW7W3Rjhn2F
rE+IpW4MeSoMdaZssCJJOz+sTnncA8E7T1s+iGuG+Rd2wXigjOl50A7w8XUX46RT
0LOiVI+o2GslUYboKoigl9+Zhrv82nmC7KCsBvJrXVJtXpnT8FWSlCY/ntxH
-----END CERTIFICATE-----
";

        const string mosquitto_org = @"
-----BEGIN CERTIFICATE-----
MIIEAzCCAuugAwIBAgIUBY1hlCGvdj4NhBXkZ/uLUZNILAwwDQYJKoZIhvcNAQEL
BQAwgZAxCzAJBgNVBAYTAkdCMRcwFQYDVQQIDA5Vbml0ZWQgS2luZ2RvbTEOMAwG
A1UEBwwFRGVyYnkxEjAQBgNVBAoMCU1vc3F1aXR0bzELMAkGA1UECwwCQ0ExFjAU
BgNVBAMMDW1vc3F1aXR0by5vcmcxHzAdBgkqhkiG9w0BCQEWEHJvZ2VyQGF0Y2hv
by5vcmcwHhcNMjAwNjA5MTEwNjM5WhcNMzAwNjA3MTEwNjM5WjCBkDELMAkGA1UE
BhMCR0IxFzAVBgNVBAgMDlVuaXRlZCBLaW5nZG9tMQ4wDAYDVQQHDAVEZXJieTES
MBAGA1UECgwJTW9zcXVpdHRvMQswCQYDVQQLDAJDQTEWMBQGA1UEAwwNbW9zcXVp
dHRvLm9yZzEfMB0GCSqGSIb3DQEJARYQcm9nZXJAYXRjaG9vLm9yZzCCASIwDQYJ
KoZIhvcNAQEBBQADggEPADCCAQoCggEBAME0HKmIzfTOwkKLT3THHe+ObdizamPg
UZmD64Tf3zJdNeYGYn4CEXbyP6fy3tWc8S2boW6dzrH8SdFf9uo320GJA9B7U1FW
Te3xda/Lm3JFfaHjkWw7jBwcauQZjpGINHapHRlpiCZsquAthOgxW9SgDgYlGzEA
s06pkEFiMw+qDfLo/sxFKB6vQlFekMeCymjLCbNwPJyqyhFmPWwio/PDMruBTzPH
3cioBnrJWKXc3OjXdLGFJOfj7pP0j/dr2LH72eSvv3PQQFl90CZPFhrCUcRHSSxo
E6yjGOdnz7f6PveLIB574kQORwt8ePn0yidrTC1ictikED3nHYhMUOUCAwEAAaNT
MFEwHQYDVR0OBBYEFPVV6xBUFPiGKDyo5V3+Hbh4N9YSMB8GA1UdIwQYMBaAFPVV
6xBUFPiGKDyo5V3+Hbh4N9YSMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEL
BQADggEBAGa9kS21N70ThM6/Hj9D7mbVxKLBjVWe2TPsGfbl3rEDfZ+OKRZ2j6AC
6r7jb4TZO3dzF2p6dgbrlU71Y/4K0TdzIjRj3cQ3KSm41JvUQ0hZ/c04iGDg/xWf
+pp58nfPAYwuerruPNWmlStWAXf0UTqRtg4hQDWBuUFDJTuWuuBvEXudz74eh/wK
sMwfu1HFvjy5Z0iMDU8PUDepjVolOCue9ashlS4EB5IECdSR2TItnAIiIwimx839
LdUdRudafMu5T5Xma182OC0/u/xRlEm+tvKGGmfFcN0piqVl8OrSPBgIlb+1IKJE
m/XriWr/Cq4h/JfB7NTsezVslgkBaoU=
-----END CERTIFICATE-----
";

        // 4cb2f8fdbf3f400b8185e21d836af81a
        // 5cb81075270640a7aedeadc85e786f0e
        // f86c1a910f1940979fadeaf785d6b474 //tls

        var input = new Input
        {
            // BrokerAddress = "test.mosquitto.org",
            BrokerAddress = "localhost", //local running mosquitto instance
            // BrokerAddress = "broker.hivemq.com", ///works best but can be laggy
            BrokerPort = 8883, // TLS
            // BrokerPort = 1883,
            ClientId = "f86c1a910f1940979fadeaf785d6b474", // starts a new session
            Topic = "example topic",
            HowLongTheTaskListensForMessages = 5,
            UseTLS12 = true,
            QoS = 2,
            Username = "username",
            Password = "derp",
            AllowInvalidCertificate = true,
        };

        // start a session to create an inbox
        var result = await MQTT.Receive(input, default);

        Assert.IsTrue(result.Success);

        if (result.Success)
        {
            // send a test message using a separate sending client (so that the sender and recipient are separate)
            var factory = new MqttClientFactory();
            using var sendingMqttClient = factory.CreateMqttClient();

            var caChain = new X509Certificate2Collection();
            caChain.ImportFromPem(mosquitto_org);
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithTrustChain(caChain);

            tlsOptions.WithCertificateValidationHandler(
                o =>
                {
                    if (o.SslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                    {
                        Console.WriteLine($"SSL/TLS Errors: {o.SslPolicyErrors}");
                    }

                    return true;
                });

            tlsOptions.WithSslProtocols(SslProtocols.Tls12);

            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(input.BrokerAddress, input.BrokerPort)
            .WithClientId("c1e06c346ef1456fb4e2b038706d9693")
            .WithCleanSession(true)
            .WithTlsOptions(tlsOptions.Build())
            .Build();

            // wait a bit, this broker has a connection rate limit
            await Task.Delay(10000);

            var senderClientConnectResult = await sendingMqttClient.ConnectAsync(options, default);

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(input.Topic)
                        .WithPayload("test message " + i)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                        .Build();

                    var sendingResult = await sendingMqttClient.PublishAsync(mqttMessage, default);
                    Assert.IsTrue(sendingResult.IsSuccess);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("test cancelled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to send MQTT message: ", ex.Message);
            }
            finally
            {
                await sendingMqttClient.DisconnectAsync(new MqttClientDisconnectOptions());
            }

            // wait 10s for the broker to permit a new connection
            await Task.Delay(10000);

            // now collect the sent message from the inbox
            input.HowLongTheTaskListensForMessages = 15;
            var resultOfReceivingMessage = await MQTT.Receive(input, default);

            foreach (var message in resultOfReceivingMessage.MessagesList)
            {
                Debug.WriteLine(message);
            }

            Assert.IsTrue(resultOfReceivingMessage.Success);
        }
    }

    [Test]
    public async Task ShouldConnectWithTLS()
    {
        var input = new Input
        {
            BrokerAddress = "localhost",
            BrokerPort = 8883,
            ClientId = "f86c1a910f1940979fadeaf785d6b474",
            Topic = "example",
            HowLongTheTaskListensForMessages = 15,
            UseTLS12 = true,
            AllowInvalidCertificate = true,
        };

        var ret = await MQTT.Receive(input, default);
        Assert.That(ret.Success, Is.True);
    }
}
