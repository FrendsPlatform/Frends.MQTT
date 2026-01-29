namespace Frends.MQTT.Receive;

using Frends.MQTT.Receive.Definitions;
using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class MQTTConnectionCreator
{
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

        MqttQualityOfServiceLevel qos = (MqttQualityOfServiceLevel)taskInput.QoS;

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(taskInput.Host, taskInput.BrokerPort)
            .WithCleanSession(false)
            .WithWillQualityOfServiceLevel(qos)
            .WithSessionExpiryInterval(sessionExpiryInterval: uint.MaxValue)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(taskInput.ReceivingTime))
            .WithClientId(clientID);

        if (taskInput.UseTls12)
        {
            var tlsOptions = new MqttClientTlsOptionsBuilder().WithCertificateValidationHandler(
                o =>
                {
                    // how do we proceed with the certificate the server sent?
                    if (o.SslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                    {
                        if (taskInput.AllowInvalidCertificate)
                            return true;
                        else
                            throw new InvalidCredentialException(o.SslPolicyErrors.ToString());
                    }
                    else
                    {
                        return true;
                    }
                });

            tlsOptions.WithSslProtocols(SslProtocols.Tls12);

            if (taskInput.UseClientCertificate)
            {
                IEnumerable<X509Certificate2> certificates = Array.Empty<X509Certificate2>();

                switch (taskInput.CertificateSource)
                {
                    case CertificateSource.CertificateStore:
                        if (taskInput.CertificateThumbprint == null)
                            throw new ArgumentException("CertificateThumbprint cannot be empty when CertificateStore is selected.");

                        certificates = LoadCertificateFromStore(
                            taskInput.CertificateThumbprint,
                            StoreName.My,
                            taskInput.CertificateStoreLocation is CertificateStoreLocation.LocalMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser);
                        break;
                    case CertificateSource.File:
                        if (taskInput.CertificateFilePath == null)
                            throw new ArgumentException("File path is required for CertificateSource.File authentication.");
                        certificates = LoadCertificateFromFile(taskInput.CertificateFilePath, taskInput.CertificateKeyFilePath, taskInput.CertificatePassword ?? string.Empty);
                        break;
                    case CertificateSource.String:
                        if (taskInput.CertificateBase64String == null)
                            throw new ArgumentException("Base64 certificate string is required for CertificateSource.String authentication.");
                        certificates = LoadCertificateFromBase64(taskInput.CertificateBase64String, taskInput.CertificatePassword ?? string.Empty);
                        break;
                    default:
                        break;
                }

                tlsOptions.WithClientCertificates(certificates);
            }

            // build TLS settings
            options.WithTlsOptions(tlsOptions.Build());
        }

        if (!string.IsNullOrEmpty(taskInput.Username) && !string.IsNullOrEmpty(taskInput.Password))
        {
            options.WithCredentials(taskInput.Username, taskInput.Password);
        }

        var messagesList = new ConcurrentQueue<string>();

        // in the future, any incoming messages will go on the list
        var handler = (Func<MqttApplicationMessageReceivedEventArgs, Task>)(e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            messagesList.Enqueue(payload);
            return Task.CompletedTask;
        });

        mqttClient.ApplicationMessageReceivedAsync += handler;

        MqttClientConnectResult connectionResponse = null;
        try
        {
            // try to CONNECT
            // if unsuccessful, this will throw an exception
            connectionResponse = await mqttClient.ConnectAsync(options.Build(), cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new Result(success: false, clientID: clientID, cException.Message, messagesList: messagesList);
        }
        catch (Exception ex)
        {
            return new Result(success: false, clientID: clientID, error: $"Error while trying to connect to MQTT broker: {ex.Message}", messagesList: messagesList);
        }

        // after connecting, immediately SUBSCRIBE
        var mqttSubscribeOptions = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
            taskInput.Topic,
            qualityOfServiceLevel: qos,
            retainAsPublished: true,
            retainHandling: MqttRetainHandling.SendAtSubscribe)
            .Build();

        try
        {
            var subscribeResponse = await mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);
        }
        catch (OperationCanceledException cException)
        {
            return new Result(success: false, clientID: clientID, $"Error while trying to connect to MQTT broker: {cException.Message}", messagesList: messagesList);
        }
        catch (Exception e)
        {
            return new Result(success: false, clientID: clientID, $"Error while trying to connect to MQTT broker: {e.Message}", messagesList: messagesList);
        }

        // collect messages for some seconds, then dispose at the curly bracket
        // close session dirty (without sending a DISCONNECT packet, to make the broker keep session.
        await Task.Delay(taskInput.ReceivingTime * 1000, cancellationToken);
        var result = new Result(
            success: true,
            clientID: clientID,
            error: string.Empty,
            messagesList: messagesList);

        mqttClient.ApplicationMessageReceivedAsync -= handler;

        return result;

        // because we used "using" when creating the client,
        // it will be disposed here and will no longer process messages or send acknowledgements/ping!
    }

    private static IEnumerable<X509Certificate2> LoadCertificateFromStore(string thumbprint, StoreName storeName, StoreLocation storeLocation)
    {
        using var store = new X509Store(storeName, storeLocation);
        store.Open(OpenFlags.ReadOnly);

        thumbprint = thumbprint.Replace(" ", string.Empty).ToUpperInvariant();

        var certs = store.Certificates
            .Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

        if (certs.Count == 0)
            throw new ArgumentException($"Certificate with thumbprint '{thumbprint.Trim()}' not found.");

        var cert = certs[0];

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("Certificate does not contain a private key.");

        return new[] { new X509Certificate2(cert) };
    }

    private static IEnumerable<X509Certificate2> LoadCertificateFromFile(string certificateFilePath, string? certificateKeyFilePath = null, string? password = null)
    {
        if (!File.Exists(certificateFilePath))
            throw new FileNotFoundException("Certificate file not found.", certificateFilePath);

        var extension = Path.GetExtension(certificateFilePath).ToLowerInvariant();

        if (extension is ".pfx" or ".p12")
        {
            // PFX/PKCS#12 file
            var cert = new X509Certificate2(
                certificateFilePath,
                password ?? string.Empty,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            if (!cert.HasPrivateKey)
                throw new InvalidCredentialException("The PFX certificate does not contain a private key.");

            return new[] { cert };
        }
        else if (extension is ".crt" or ".pem")
        {
            // PEM certificate. Must have key file
            if (string.IsNullOrEmpty(certificateKeyFilePath))
                throw new ArgumentException("Private key file path is required for PEM certificates.");

            if (!File.Exists(certificateKeyFilePath))
                throw new FileNotFoundException("Private key file not found.", certificateKeyFilePath);

            var cert = X509Certificate2.CreateFromPemFile(certificateFilePath, certificateKeyFilePath);

            if (!cert.HasPrivateKey)
                throw new InvalidCredentialException("The PEM certificate or key is invalid or missing the private key.");

            return new[] { cert };
        }
        else
        {
            throw new NotSupportedException($"Unsupported certificate file extension: {extension}");
        }
    }

    private static IEnumerable<X509Certificate2> LoadCertificateFromBase64(string base64, string password)
    {
        var raw = Convert.FromBase64String(base64);

        var cert = new X509Certificate2(
                raw,
                password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        if (!cert.HasPrivateKey)
            throw new InvalidCredentialException("Certificate does not contain a private key.");

        return new[] { cert };
    }
}
