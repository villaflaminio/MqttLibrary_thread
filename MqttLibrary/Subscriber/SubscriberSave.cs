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
            ///https://github-wiki-see.page/m/chkr1011/MQTTnet/wiki/Client
            ///
            Dispatcher dispatcher = Dispatcher.GetInstance(10);

            MqttFactory mqttFactory = new MqttFactory();

            X509Certificate caCert = X509Certificate.CreateFromCertFile(@"C:\certs\CACERT.crt");
            X509Certificate clientCert = new X509Certificate2(@"C:\certs\certificateClient.pfx", "flaminio");

            IMqttClient client = mqttFactory.CreateMqttClient();
            var tlsOptions = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                Certificates = new List<X509Certificate>
                        {
                            clientCert, caCert

                        },
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true
            };

            var options = new MqttClientOptionsBuilder()
                            .WithClientId(Guid.NewGuid().ToString())
                            .WithTcpServer("localhost", 8883)
                            .WithCredentials("root", "flaminio")
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
                MessageMqtt message = new MessageMqtt(Encoding.UTF8.GetString(e.ApplicationMessage.Payload), e.ApplicationMessage.Topic, DateTime.Now);
                //message.Payload += " id Subscriber: " + id;
                dispatcher.AddRequest(message);


            });
            client.ConnectAsync(options);



        }
    }
}
