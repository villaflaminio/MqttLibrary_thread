using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLibrary.Subscriber.repository
{

    public class RepositoryService
    {
        private static RepositoryService _instance;
        private static Repository db = new Repository();
        private static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        private readonly static object listLock = new object();
        private Thread processRequestThread = new Thread(ProcessRequest);
        private RepositoryService() {
            processRequestThread.Start();
        }

        public static RepositoryService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RepositoryService();
            }
            return _instance;
        }

        public void SaveMessage(MessageMqtt messageDeque)
        {
            lock (listLock)
            {
                messageQueue.Enqueue(messageDeque);
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }           
        }

        static void ProcessRequest()
        {
            while (true)
            {
                try
                {
                    lock (listLock)
                    {
                        ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                        while (messageQueue.Count == 0)
                        {
                            ///rilascio listLock, riacquistandolo solo dopo essere stato svegliato da una chiamata a Pulse
                            Monitor.Wait(listLock);
                        }
                        MessageMqtt messageDeque = messageQueue.Dequeue();
                        messageDeque.Id = null; // l'id deve essere generato automaticamente da repository
                        db.Messages.Add(messageDeque);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

        }
    }
}
