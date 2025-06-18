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
            .WithTcpServer("localhost", 1883)
            .WithClientId("Standard_Client")
            .Build();

        Console.WriteLine("🚀 Starting standard MQTT client ... 🚀");

        client.ConnectedAsync += args =>
        {
            Console.WriteLine("✅ Connected to MQTT broker.");
            return Task.CompletedTask;
        };

        client.DisconnectedAsync += args =>
        {
            Console.WriteLine("❌ Disconected from MQTT broker.");
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
            Console.WriteLine($"📤 Published: {payload}");

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
