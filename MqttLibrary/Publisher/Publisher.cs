﻿using System;
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
    public class Publisher
    {
        private static int[] requestPerSecond = { 100, 45, 20, 15, 1 };

        public static async void Run()

        {   ///https://github-wiki-see.page/m/chkr1011/MQTTnet/wiki/Client
            
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
            Random random = new Random();
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
