using MQTTnet;
using MQTTnet.Client;
using System.Text;


Console.WriteLine("-- MQTT Subscriber --");
var mqttFactory = new MqttFactory();
using var mqttClient = mqttFactory.CreateMqttClient();
var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("192.168.1.7", 1883)
                        .WithCredentials("homeassistant", "thaphie2ipoo0fooghooch7ahLaecoo2aes8reeleetheengoul5ahraen7rootu")
                        .WithCleanSession()
                        .Build();

mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
    .WithTopicFilter(
        f => {
            f.WithTopic("homeassistant/#");
        })
    .Build();

await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

Console.WriteLine("MQTT client subscribed to topic.");
Thread.Sleep(-1);

static async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e) {
Console.WriteLine($"# Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
await Task.CompletedTask;
}