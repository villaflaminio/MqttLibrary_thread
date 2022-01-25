using MqttLibrary.Subscriber.repository;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLibrary.Worker
{
    public class Worker
    {

        private static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        private RepositoryService repository = RepositoryService.GetInstance();
        readonly static object listLock = new object();
        private Thread mainThread;
        public void Start()
        {
            mainThread = new Thread(WorkerThread);
            mainThread.Start();
        }

    

        public void WorkerThread()
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
                        //messageDeque.Payload += " Worked";
                        Console.WriteLine("Saving -> " + messageDeque.ToString());
                        repository.SaveMessage(messageDeque);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
            ///devo finire di svuotare la coda prima di arrestare il Thread

        }

        public void AddData(MessageMqtt m)
        {
            lock (listLock)
            {
                messageQueue.Enqueue(m);
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }
        }

        public void Stop()
        {
            new Thread(() =>
            {
                try
                {
                    mainThread.Interrupt();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                while (messageQueue.Count > 0)
                {
                    MessageMqtt messageDeque = messageQueue.Dequeue();
                    repository.SaveMessage(messageDeque);
                }
            }).Start();
        }
    }
}
