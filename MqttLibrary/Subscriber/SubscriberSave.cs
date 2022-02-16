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
    //public class SubscriberSave
    //{
    //    private static X509Certificate caCert;
    //    public static void Run()
    //    {
    //        var mqttFactory = new MqttFactory();

    //        IMqttClient client = mqttFactory.CreateMqttClient();
    //        var tlsOptions = new MqttClientOptionsBuilderTlsParameters
    //        {
    //            UseTls = true,
    //            Certificates = new List<X509Certificate>
    //            {
    //                new X509Certificate("C:\\work_space\\c#\\2.mqtt_dispatcher\\MqttLibrary\\MqttLibrary\\Subscriber\\mosquitto.org.crt")
    //            },
    //            AllowUntrustedCertificates = true,
    //            IgnoreCertificateChainErrors = true,
    //            IgnoreCertificateRevocationErrors = true
    //        };
    //        var options = new MqttClientOptionsBuilder()
    //                        .WithClientId(Guid.NewGuid().ToString())
    //                        .WithTcpServer("test.mosquitto.org", 8883)
    //                        .WithTls(tlsOptions)
    //                        .WithCleanSession()
    //                        .Build();

    //        client.UseConnectedHandler(e =>
    //        {

    //            Console.WriteLine("subscriber save connesso");
    //            var topicFilter = new TopicFilterBuilder()
    //            .WithTopic("flaminio")
    //            .Build();
    //            client.SubscribeAsync(topicFilter);

    //        });

    //        client.UseDisconnectedHandler(e =>
    //        {
    //            Console.WriteLine("Disconnect");
    //        });

    //        client.UseApplicationMessageReceivedHandler(e =>
    //        {
    //            var message = new MessageMqtt(Encoding.UTF8.GetString(e.ApplicationMessage.Payload), e.ApplicationMessage.Topic, DateTime.Now);
    //            //message.Payload += " id Subscriber: " + id;
    //            Console.WriteLine(message);


    //        });
    //        client.ConnectAsync(options);
    //    }


    //    //public static void Run2()
    //    //{

    //    //    // Dispatcher dispatcher = Dispatcher.GetInstance(10);
    //    //    Dispatcher dispatcher = Dispatcher.GetInstance(10);

    //    //    //var id = Guid.NewGuid().ToString();
    //    //    //var mqttFactory = new MqttFactory();
    //    //    //var client = mqttFactory.CreateMqttClient();
    //    //    ////var otions = new MqttClientOptionsBuilder()
    //    //    ////    .WithClientId(id)
    //    //    ////    .WithTcpServer("localhost", 1883)
    //    //    ////    .WithCleanSession()
    //    //    ////    .Build();

    //    //    // var clientCert = new X509Certificate2("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt");
    //    //    //var clientCert = new X509Certificate2("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt");
    //    //    //Console.WriteLine(clientCert.ToString());
    //    //    //Console.WriteLine(clientCert.HasPrivateKey);
    //    //    //var options = new MqttClientOptionsBuilder()
    //    //    //    .WithClientId(id)
    //    //    //    .WithTcpServer("50-6-102", 8883)
    //    //    //    .WithTls(new MqttClientOptionsBuilderTlsParameters()
    //    //    //    {
    //    //    //        UseTls = true,
    //    //    //        AllowUntrustedCertificates = true,
    //    //    //        IgnoreCertificateChainErrors = true,
    //    //    //        IgnoreCertificateRevocationErrors = true,
    //    //    //        SslProtocol = SslProtocols.Tls,
    //    //    //        Certificates = new List<X509Certificate>()
    //    //    //            {
    //    //    //                caCert, clientCert
    //    //    //            }
    //    //    //    })
    //    //    //    .WithCleanSession()
    //    //    //    .WithProtocolVersion(MqttProtocolVersion.V311)
    //    //    //    .Build();

    //    //    //MqttClient client = new MqttClient("50-6-102", 8883,true, clientCert, new X509Certificate(Resources.m2mqtt_ca), MqttSslProtocols.None);

    //    //    //MqttClient mqttClient = new MqttClient("50-6-102",
    //    //    //                    uPLibrary.Networking.M2Mqtt.MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT,
    //    //    //                    true,
    //    //    //                    X509Certificate.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_ca.der"),
    //    //    //                    X509Certificate2.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt"),
    //    //    //                    MqttSslProtocols.SSLv3);
    //    //    X509Certificate certRootCa = X509Certificate.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt");
    //    //    X509Certificate2 certClient = new X509Certificate2("C:\\Program Files\\OpenSSL-Win64\\bin\\custom_m2mqtt.pfx", "flaminio");

    //    //    MqttClient mqttClient = new MqttClient("50-6-102", 8883, true, certRootCa, certClient, MqttSslProtocols.SSLv3);

    //    //    //client.UseConnectedHandler(e =>
    //    //    //{

    //    //    //    Console.WriteLine("subscriber save connesso");
    //    //    //    var topicFilter = new TopicFilterBuilder()
    //    //    //    .WithTopic("flaminio")
    //    //    //    .Build();
    //    //    //    client.SubscribeAsync(topicFilter);

    //    //    //});

    //    //    //client.UseDisconnectedHandler(e =>
    //    //    //{
    //    //    //    Console.WriteLine("Disconnect");
    //    //    //});

    //    //    //client.UseApplicationMessageReceivedHandler(e =>
    //    //    //{
    //    //    //    var message = new MessageMqtt(Encoding.UTF8.GetString(e.ApplicationMessage.Payload), e.ApplicationMessage.Topic, DateTime.Now);
    //    //    //    //message.Payload += " id Subscriber: " + id;
    //    //    //    dispatcher.AddRequest(message);


    //    //    //});
    //    //    //client.ConnectAsync(options);

    //    //    //client.MqttMsgPublishReceived += MessageReceived;
    //    //    string clientId = Guid.NewGuid().ToString();
    //    //    mqttClient.Connect(clientId);
    //    //    // Register the message received using the event MqttMsgPublishReceived.
    //    //    mqttClient.MqttMsgPublishReceived += client_receivedMessage;

    //    //    // Set the client Id. 


    //    //    // Print the subscriber datas about the listened topic.
    //    //    Console.WriteLine("Topic: Data/");

    //    //    // Subscribe Client to Server --> topic "Data/".
    //    //    mqttClient.Subscribe(new String[] { "Data/" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    //    //}


    //    //// Method called when there is a new message to read. 
    //    //// It receives the message and insert it into DB.
    //    //static void client_receivedMessage(object sender, MqttMsgPublishEventArgs e)
    //    //{
    //    //    // Handle message received. 
    //    //    String message = System.Text.Encoding.Default.GetString(e.Message);

    //    //    // Create message to Add Request. 
    //    //    //MessageMqtt messageObj = new MessageMqtt
    //    //    //{
    //    //    //    dateTime = DateTime.Now,
    //    //    //    payload = message
    //    //    //};

    //    //    //dispatcher.AddRequest(messageObj);
    //    //    Console.WriteLine(message);

    //    //    // Inserimento del messaggio nel DB. 
    //    //    // System.Console.WriteLine("Inserimento nel DB del messaggio: " + message);
    //    //    // insertValueIntoMySQLDB(message);
    //    //    // insertValueIntoMicrosoftSQLDB(message);
    //    //}



    //}





    public class SubscriberSave
    {

        public static void Run()
        {

            // Dispatcher dispatcher = Dispatcher.GetInstance(10);
            Dispatcher dispatcher = Dispatcher.GetInstance(10);

            var id = Guid.NewGuid().ToString();
            var mqttFactory = new MqttFactory();
            //var client = mqttFactory.CreateMqttClient();
            //var otions = new MqttClientOptionsBuilder()
            //    .WithClientId(id)
            //    .WithTcpServer("localhost", 188)
            //    .WithCleanSession()
            //    .Build();


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
