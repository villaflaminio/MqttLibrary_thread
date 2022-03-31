using MqttApp;
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
    public delegate void Notify();  // delegate

    public class Worker 
    {

        private static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        private RepositoryService repository = RepositoryService.GetInstance();
        readonly static object listLock = new object();
        private Thread mainThread;
        private DateTime millis;
        private int worker_code;
        public event EventHandler<int> ProcessKilled;
        private bool running; 


        
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
                    lock (listLock)
                    {
                        ///Se la coda è vuota, e non ricevo nuovi dati da almeno secondi
                        if (millis.AddSeconds(5) < DateTime.Now)
                        {
                            OnProcessKilled();
                            Stop();
                            break;

                        }
                        else if(messageQueue.Count != 0)
                        {
                            MessageMqtt messageDeque = messageQueue.Dequeue();
                            //messageDeque.Payload += " Worked";
                            Console.WriteLine("Saving -> " + messageDeque.ToString());
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
            lock (listLock)
            {
                messageQueue.Enqueue(m);
                millis = DateTime.Now;
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }
        }

        public void Stop()
        {
           Console.WriteLine("STOPPPP");
            new Thread(() =>
            {
                try
                {
                    running = false;
                    OnProcessKilled();

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



        public string getTipo()
        {
            return ("special_worker");
        }
    }
}
