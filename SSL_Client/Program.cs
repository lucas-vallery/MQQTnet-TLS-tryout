using System.Text;
using MQTTnet;
using MQTTnet.Protocol;

class Program
{
    static async Task Main()
    {
        var factory = new MqttClientFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 8883) // TLS port
            .WithClientId("SSL_Client")
            .WithTlsOptions(new MqttClientTlsOptions
            {
                UseTls = true,
                AllowUntrustedCertificates = true, // For self-signed certs
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                CertificateValidationHandler = context =>
                {
                    Console.WriteLine($"🔐 TLS cert from {context.Certificate.Subject} accepted.");
                    return true;
                }
            })
            .Build();

        Console.WriteLine("🚀 Starting secure MQTT client... 🚀");

        client.ConnectedAsync += args =>
        {
            Console.WriteLine("✅ Connected to MQTT broker over TLS.");
            return Task.CompletedTask;
        };

        client.DisconnectedAsync += args =>
        {
            Console.WriteLine("❌ Disconnected from MQTT broker.");
            return Task.CompletedTask;
        };

        await client.ConnectAsync(options);

        while (true)
        {
            var payload = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var message = new MqttApplicationMessage
            {
                Topic = "system/time/now",
                PayloadSegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(payload)),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = true
            };

            await client.PublishAsync(message);
            Console.WriteLine($"📤 Published (TLS): {payload}");

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
