using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MqttLibrary.Publisher
{
    //public class Publisher
    //{
    //    private static int[] requestPerSecond = { 100, 45, 20, 15, 1 };

    //    //public static async void Run()

    //    //{
    //    //    var mqttFactory = new MqttFactory();

    //    //    var client = mqttFactory.CreateMqttClient();
    //    //    //var otions = new MqttClientOptionsBuilder()
    //    //    //    .WithClientId(Guid.NewGuid().ToString())
    //    //    //    .WithTcpServer("localhost", 1883)
    //    //    //    .WithCleanSession()
    //    //    //    .Build();
    //    //   // var clientCert = new X509Certificate(Resources.m2mqtt_ca);
    //    //   var clientCert = new X509Certificate2("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt");
    //    //    Console.WriteLine(clientCert.ToString());
    //    //    Console.WriteLine(clientCert.HasPrivateKey);
    //    //    var options = new MqttClientOptionsBuilder()
    //    //        .WithClientId(Guid.NewGuid().ToString())
    //    //        .WithTcpServer("50-6-102", 8883)
    //    //        .WithTls(new MqttClientOptionsBuilderTlsParameters()
    //    //        {
    //    //            UseTls = true,
    //    //            AllowUntrustedCertificates = true,
    //    //            IgnoreCertificateChainErrors = true,
    //    //            IgnoreCertificateRevocationErrors = true,
    //    //            SslProtocol = SslProtocols.Tls,
    //    //            Certificates = new List<X509Certificate>()
    //    //                {
    //    //                    caCert, clientCert
    //    //                }
    //    //        })
    //    //        .WithCleanSession()
    //    //        .WithProtocolVersion(MqttProtocolVersion.V311)
    //    //        .Build();

    //    //    client.UseConnectedHandler(e =>
    //    //    {
    //    //        Console.WriteLine("publisher connected to broker");
    //    //    });
    //    //    client.UseDisconnectedHandler(e =>
    //    //    {
    //    //        Console.WriteLine(mqttFactory.GetHashCode + "Disconnect");
    //    //    });

    //    //    //await client.ConnectAsync(options);

    //    //    try
    //    //    {
    //    //        await client.ConnectAsync(options, CancellationToken.None);
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        Console.WriteLine(ex);
    //    //    }

    //    //    while (!client.IsConnected)
    //    //    {
    //    //        Task.Delay(100).Wait();
    //    //        Console.WriteLine(mqttFactory.GetHashCode + "Disconnect");
    //    //    }

    //    //    ///
    //    //    /// Test invio dati verso i subscriber
    //    //    ///
    //    //    for (int i = 1; i < 700; i++)
    //    //    {
    //    //        ///
    //    //        /// [0] = 10 req/s
    //    //        /// [1] = 20 req/s
    //    //        /// [2] = 66 req/s
    //    //        /// [3] = 50 req/s
    //    //        /// [4] = 60 req/s
    //    //        ///
    //    //        if (i < 100) // 20
    //    //        {
    //    //            PublishMessageAsync(client, i);
    //    //            Task.Delay(requestPerSecond[2]).Wait();
    //    //        }
    //    //        else if (i >= 100 && i < 200)
    //    //        {
    //    //            PublishMessageAsync(client, i);
    //    //            Task.Delay(requestPerSecond[4]).Wait();
    //    //        }
    //    //        else if (i >= 200 && i < 300)
    //    //        {
    //    //            PublishMessageAsync(client, i);
    //    //            Task.Delay(requestPerSecond[4]).Wait();
    //    //        }
    //    //        else if (i >= 300 && i < 400)
    //    //        {
    //    //            PublishMessageAsync(client, i);
    //    //            Task.Delay(requestPerSecond[1]).Wait();
    //    //        }
    //    //        else 
    //    //        {
    //    //            PublishMessageAsync(client, i);
    //    //            //Task.Delay(requestPerSecond[4]).Wait();
    //    //        }
    //    //    }


    //    //}

    //    public static async void Run()
    //    {
    //        //        X509Certificate certRootCa = X509Certificate.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt");
    //        //    X509Certificate2 certClient = new X509Certificate2("C:\\Program Files\\OpenSSL-Win64\\bin\\custom_m2mqtt.pfx", "flaminio");

    //        //    MqttClient client = new MqttClient("50-6-102", 8883, true, certRootCa, certClient, MqttSslProtocols.SSLv3);


    //        //    //MqttClient client = new MqttClient("50-6-102",
    //        //    //                    uPLibrary.Networking.M2Mqtt.MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT,
    //        //    //                    true,
    //        //    //                    X509Certificate.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_ca.der"),
    //        //    //                    X509Certificate2.CreateFromCertFile("C:\\Program Files\\OpenSSL-Win64\\bin\\m2mqtt_srv.crt"), 
    //        //    //                    MqttSslProtocols.SSLv3 );

    //        //    string clientId = Guid.NewGuid().ToString();
    //        //    client.Connect(clientId);
    //        //    // Create a test string.
    //        //    string text = "messaggio_";
    //        //    for (int j = 0; j < 100; j++)
    //        //    {
    //        //        for (int i = 0; i < 20; i++)
    //        //        {
    //        //            Console.WriteLine(DateTime.Now + " | Publisher message: " + text + i);



    //        //            // Publish the message to "Data/" topic and increment messageCounter.
    //        //            client.Publish("Data/", Encoding.UTF8.GetBytes(text), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
    //        //            Task.Delay(1).Wait(); // Wait 1 millis.
    //        //        }
    //        //        // Thread.Sleep(1000);
    //        //    }
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
    //            Console.WriteLine("Connected to the broker");
    //        });

    //        client.UseDisconnectedHandler(e =>
    //        {
    //            Console.WriteLine("Disconnected from the broker");
    //        });

    //        await client.ConnectAsync(options);
    //        while (!client.IsConnected)
    //        {
    //            Task.Delay(100).Wait();
    //            Console.WriteLine(mqttFactory.GetHashCode + "Disconnect");
    //        }
    //        //Console.WriteLine("Please press a key to publish the message");

    //        //Console.ReadLine();

    //        //await PublishMessageAsync(client);

    //        //await client.DisconnectAsync();
    //        PublishMessageAsync(client, 10);

    //    }
    //    //private static async Task PublishMessageAsync(IMqttClient client)
    //    //{
    //    //    string messagePayload = "Hello!";
    //    //    var message = new MqttApplicationMessageBuilder()
    //    //                        .WithTopic("RishabhSharma")
    //    //                        .WithPayload(messagePayload)
    //    //                        .WithAtLeastOnceQoS()
    //    //                        .Build();
    //    //    if (client.IsConnected)
    //    //    {
    //    //        await client.PublishAsync(message);
    //    //        Console.WriteLine($"Published Message - {messagePayload}");
    //    //    }
    //    //}

    //    private static async Task PublishMessageAsync(IMqttClient client, int i)
    //    {
    //        String[] typeOfWorker = { "worker_a", "worker_b", "worker" };
    //        String[] typeOfInterface = { "IWorker", "ISpecialWorker" };
    //        var random = new Random();
    //        int randomIndextypeOfWorker = random.Next(0, typeOfWorker.Length);
    //        int randomIndextypeOfInterface = random.Next(0, typeOfWorker.Length);

    //        //string mex = "Hello " + i + " Time_Send: " + DateTime.Now;Random rn = new Random();
    //        string mex = i + "";

    //        var message = new MqttApplicationMessageBuilder()
    //            .WithTopic("flaminio")
    //            .WithPayload(typeOfInterface[randomIndextypeOfInterface] + "," + typeOfWorker[randomIndextypeOfWorker])
    //            .WithAtLeastOnceQoS()
    //            .Build();

    //        if (client.IsConnected)
    //        {
    //            await client.PublishAsync(message);
    //        }

    //    }
    //}





    public class Publisher
    {
        private static int[] requestPerSecond = { 100, 45, 20, 15, 1 };

        public static async void Run()

        {
            var mqttFactory = new MqttFactory();

            var client = mqttFactory.CreateMqttClient();
            //var otions = new MqttClientOptionsBuilder()
            //    .WithClientId(Guid.NewGuid().ToString())
            //    .WithTcpServer("localhost", 1884)
            //    .WithCleanSession()
            //    .Build();
            //var client = mqttFactory.CreateMqttClient();
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
                Console.WriteLine("publisher connected to broker");
            });
            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine(mqttFactory.GetHashCode + "Disconnect");
            });

            await client.ConnectAsync(options);


            while (!client.IsConnected)
            {
                Task.Delay(100).Wait();
                Console.WriteLine(mqttFactory.GetHashCode + "Disconnect");
            }

            ///
            /// Test invio dati verso i subscriber
            ///
            for (int i = 1; i < 700; i++)
            {
                ///
                /// [0] = 10 req/s
                /// [1] = 20 req/s
                /// [2] = 66 req/s
                /// [3] = 50 req/s
                /// [4] = 60 req/s
                ///
                if (i < 100) // 20
                {
                    PublishMessageAsync(client, i);
                    Task.Delay(requestPerSecond[2]).Wait();
                }
                else if (i >= 100 && i < 200)
                {
                    PublishMessageAsync(client, i);
                    Task.Delay(requestPerSecond[4]).Wait();
                }
                else if (i >= 200 && i < 300)
                {
                    PublishMessageAsync(client, i);
                    Task.Delay(requestPerSecond[4]).Wait();
                }
                else if (i >= 300 && i < 400)
                {
                    PublishMessageAsync(client, i);
                    Task.Delay(requestPerSecond[1]).Wait();
                }
                else
                {
                    PublishMessageAsync(client, i);
                    //Task.Delay(requestPerSecond[4]).Wait();
                }
            }


        }

        private static async Task PublishMessageAsync(IMqttClient client, int i)
        {
            String[] typeOfWorker = { "worker_a", "worker_b", "worker" };
            String[] typeOfInterface = { "IWorker", "ISpecialWorker" };
            var random = new Random();
            int randomIndextypeOfWorker = random.Next(0, typeOfWorker.Length);
            int randomIndextypeOfInterface = random.Next(0, typeOfWorker.Length);

            //string mex = "Hello " + i + " Time_Send: " + DateTime.Now;Random rn = new Random();
            string mex = i + "";

            var message = new MqttApplicationMessageBuilder()
                .WithTopic("flaminio")
                .WithPayload(typeOfInterface[randomIndextypeOfInterface] + "," + typeOfWorker[randomIndextypeOfWorker])
                .WithAtLeastOnceQoS()
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(message);
            }

        }
    }
}
