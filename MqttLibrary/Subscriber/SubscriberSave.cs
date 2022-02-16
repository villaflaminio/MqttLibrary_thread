using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using MqttSubscriber.Dispatcher;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using MQTTnet.Client;
using MQTTnet.Client.Options;

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
            var tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                Certificates = new List<X509Certificate>
                        {
                            new X509Certificate("C:\\work_space\\c#\\2.mqtt_dispatcher\\MqttLibrary\\MqttLibrary\\Subscriber\\client.crt")

                        },
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true
            };

            var options = new MqttClientOptionsBuilder()
                            .WithClientId(Guid.NewGuid().ToString())
                            .WithTcpServer("test.mosquitto.org", 8883)
                            .WithTls(tlsOptions)
                            .WithCleanSession()
                            .Build();


            client.UseConnectedHandler(e =>
            {

                Console.WriteLine("subscriber save connesso con mosquitto");
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
                //message.Payload += " id Subscriber: " + id;
                dispatcher.AddRequest(message);


            });
            client.ConnectAsync(options);



        }
    }
}
