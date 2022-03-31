using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections.Concurrent;
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
        private static ConcurrentQueue<MessageMqtt> _messageQueue = new ConcurrentQueue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        static SemaphoreSlim _messageQueueAvailable = new SemaphoreSlim(0);
        private static DateTime millis;

        private Thread processRequestThread = new Thread(ProcessRequest);
        private RepositoryService()
        {
            processRequestThread.Start();
        }

        public static RepositoryService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new RepositoryService();
                millis = DateTime.Now;

            }
            return _instance;
        }

        public void SaveMessage(MessageMqtt messageDeque)
        {
            _messageQueue.Enqueue(messageDeque);
            _messageQueueAvailable.Release(1);
        }

        static void ProcessRequest()
        {
            while (true)
            {
                try
                {

                    if (!_messageQueue.IsEmpty)
                    {
                        MessageMqtt messageDeque;
                        if (_messageQueue.TryDequeue(out messageDeque))
                        {
                            messageDeque.Id = null; // l'id deve essere generato automaticamente da repository
                            db.Messages.Add(messageDeque);

                            if (millis.AddSeconds(2) < DateTime.Now)
                            {
                                db.SaveChanges();
                                millis = DateTime.Now;
                            }
                        }
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
