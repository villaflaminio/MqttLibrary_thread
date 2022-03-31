using MqttApp;
using MqttLibrary.Subscriber.repository;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLibrary.Worker
{
    public delegate void Notify();  // delegate

    public class Worker
    {

        //private static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        static ConcurrentQueue<MessageMqtt> _messageQueue = new ConcurrentQueue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        static SemaphoreSlim _messageQueueAvailable = new SemaphoreSlim(0);

        private static RepositoryService repository = RepositoryService.GetInstance();
        private Thread mainThread;
        private DateTime millis;
        private int worker_code;
        public event EventHandler<int> ProcessKilled;
        private static bool running;



        public Worker(int worker_code)
        {
            this.worker_code = worker_code;
            running = true;
        }


        public void Start()
        {
            mainThread = new Thread(WorkerThread);
            millis = DateTime.Now;
            mainThread.Start();
        }

        protected virtual void OnProcessKilled() //protected virtual method
        {
            //if ProcessCompleted is not null then call delegate
            ProcessKilled?.Invoke(this, worker_code);
        }

        public void WorkerThread()
        {
            while (running)
            {
                try
                {

                    ///Se la coda è vuota, e non ricevo nuovi dati da almeno secondi
                    if (millis.AddSeconds(5) < DateTime.Now)
                    {
                        Stop();
                        OnProcessKilled();
                        break;
                    }

                    else if (!_messageQueue.IsEmpty)
                    {
                        MessageMqtt messageDeque;
                        if (_messageQueue.TryDequeue(out messageDeque))
                        {
                            //messageDeque.Payload += " Worked";
                            Console.WriteLine("Saving now -> " + messageDeque.ToString());
                            repository.SaveMessage(messageDeque);
                        }
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
            _messageQueue.Enqueue(m);
            _messageQueueAvailable.Release(1);
            millis = DateTime.Now;

        }
        public void Stop()
        {
            OnProcessKilled();
            Thread stop = new Thread(new ThreadStart(Worker.DOStop));
            stop.Start();


        }

        public static void DOStop()
        {
            Console.WriteLine("***Stopping***");

            try
            {
                running = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            MessageMqtt messageDeque;

            while (_messageQueue.TryDequeue(out messageDeque))
            {
                //messageDeque.Payload += " Worked";
                Console.WriteLine("Saving now -> " + messageDeque.ToString());
                repository.SaveMessage(messageDeque);
            }

        }



        public string getTipo()
        {
            return ("special_worker");
        }
    }
}
