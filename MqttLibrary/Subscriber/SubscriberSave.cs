using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using MqttSubscriber.Dispatcher;

namespace MqttLibrary.Subscriber
{
    public class SubscriberSave
    {

        public static void Run()
        {
            
            Dispatcher dispatcher = Dispatcher.GetInstance(10);

            var id = Guid.NewGuid().ToString();
        var mqttFactory = new MqttFactory();
            var client = mqttFactory.CreateMqttClient();
            var otions = new MqttClientOptionsBuilder()
                .WithClientId(id)
                .WithTcpServer("localhost", 1884)
                .WithCleanSession()
                .Build();

            client.UseConnectedHandler(e =>
            {

                Console.WriteLine("subscriber save connesso");
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
                var message = new MessageMqtt(Encoding.UTF8.GetString(e.ApplicationMessage.Payload), e.ApplicationMessage.Topic, DateTime.Now);
                message.Payload += " id Subscriber: " + id;
                dispatcher.AddRequest(message);


            });
            client.ConnectAsync(otions);



        }
    }
}
