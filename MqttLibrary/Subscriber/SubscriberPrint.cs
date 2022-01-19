using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace MqttLibrary.Subscriber
{
    public class SubscriberPrint
    {

        public static void Run()
        {
            var mqttFactory = new MqttFactory();
            var client = mqttFactory.CreateMqttClient();
            var otions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer("localhost", 1884)
                .WithCleanSession()
                .Build();

            client.UseConnectedHandler(e =>
            {
                
                Console.WriteLine( "Disconnect");
                var topicFilter = new TopicFilterBuilder()
                .WithTopic("flaminio")
                .Build();
                client.SubscribeAsync(topicFilter);

            });
            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnect");
            });

            client.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine();

                //Console.WriteLine($"Ricevuto: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            });

           
            client.ConnectAsync(otions);

        }
    }
}
