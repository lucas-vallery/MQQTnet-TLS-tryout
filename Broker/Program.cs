using MQTTnet.Server;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MQTTnet.Samples.Server;

public static class Server_TLS_Samples
{
    public static async Task Run_Server_With_Self_Signed_Certificate()
    {
        var mqttServerFactory = new MqttServerFactory();

        var certificate = CreateSelfSignedCertificate("1.3.6.1.5.5.7.3.1");

        var mqttServerOptions = new MqttServerOptionsBuilder()
                                    .WithDefaultEndpoint()                 
                                    .WithDefaultEndpointPort(1883)
                                    .WithEncryptionCertificate(certificate)
                                    .WithEncryptedEndpoint()
                                    .WithEncryptedEndpointPort(8883)
                                    .Build();

        Console.WriteLine("🚀 Starting MQTT broker... 🚀");
        Console.WriteLine("This is listening on:");
        Console.WriteLine("  - 1883 port for standard connections");
        Console.WriteLine("  - 8883 port for SSl/TLS encrypted connections");

        using (var mqttServer = mqttServerFactory.CreateMqttServer(mqttServerOptions))
        {
            mqttServer.ClientConnectedAsync += args =>
            {
                Console.WriteLine($"✅ Client connected: {args.ClientId}");
                return Task.CompletedTask;
            };

            mqttServer.ClientDisconnectedAsync += args =>
            {
                Console.WriteLine($"❌ Client disconnected: {args.ClientId}");
                return Task.CompletedTask;
            };

            mqttServer.ClientSubscribedTopicAsync += args =>
            {
                Console.WriteLine($"📡 Client '{args.ClientId}' subscribed to '{args.TopicFilter.Topic}'");
                return Task.CompletedTask;
            };

            mqttServer.InterceptingPublishAsync += args =>
            {
                var payload = args.ApplicationMessage?.Payload == null
                    ? "<empty>"
                    : Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

                Console.WriteLine($"📨 [PUBLISH] Client: {args.ClientId}, Topic: {args.ApplicationMessage.Topic}, Payload: {payload}");
                return Task.CompletedTask;
            };

            await mqttServer.StartAsync();

            Console.ReadLine();

            // Stop and dispose the MQTT server if it is no longer needed!
            await mqttServer.StopAsync();
        }
    }

    static X509Certificate2 CreateSelfSignedCertificate(string oid)
    {
        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        sanBuilder.AddDnsName("localhost");

        using (var rsa = RSA.Create())
        {
            var certRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

            certRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

            certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(oid) }, false));

            certRequest.CertificateExtensions.Add(sanBuilder.Build());

            using (var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddMinutes(-10), DateTimeOffset.Now.AddMinutes(10)))
            {
                var pfxCertificate = new X509Certificate2(
                    certificate.Export(X509ContentType.Pfx),
                    (string)null!,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

                return pfxCertificate;
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        await MQTTnet.Samples.Server.Server_TLS_Samples.Run_Server_With_Self_Signed_Certificate();
    }
}