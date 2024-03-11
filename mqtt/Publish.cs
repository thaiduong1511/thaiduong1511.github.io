// See https://aka.ms/new-console-template for more information
using MQTTnet;
using MQTTnet.Client;

Console.WriteLine("-- MQTT Publisher --");
var mqttFactory = new MqttFactory();
using var mqttClient = mqttFactory.CreateMqttClient();
var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("192.168.1.7",1883)
    .WithCredentials("homeassistant", "thaphie2ipoo0fooghooch7ahLaecoo2aes8reeleetheengoul5ahraen7rootu")
    .WithCleanSession()
    .Build();

await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
if (mqttClient.IsConnected)
{
    while (true) {
    Console.Write("Message >>> ");
    var payload = Console.ReadLine();
    if (payload == "quit") break;

    var applicationMessage = new MqttApplicationMessageBuilder()
    .WithTopic("homeassistant/switch/1/power")
    .WithPayload(payload)
    .Build();

    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    await mqttClient.DisconnectAsync();
}
else 
{
    Console.WriteLine("Can not connect");
}

