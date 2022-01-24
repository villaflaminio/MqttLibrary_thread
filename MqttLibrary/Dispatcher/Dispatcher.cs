using MqttLibrary.Dispatcher;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttSubscriber.Dispatcher
{

    public class Dispatcher
    {

        static private int calls; // numero di chiamate di AddRequest ricevute 
        static private Stopwatch sw;
        static private int requestPerSecond;


        static private int nThreadMax;


        static Queue<MessageMqtt> queueMessage = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        //static Queue<Thread> threads = new Queue<Thread>(); //contiene tutti i thread



        //private static Dictionary<long, Queue<MessageMqtt>> mapMessage = new Dictionary<long, Queue<MessageMqtt>>();
        private static Dictionary<long, Worker> mapThread = new Dictionary<long, Worker>();


        readonly static object listLock = new object();
        Thread dispatcherThread = new Thread(DispatcherThread);


        public Dispatcher(int nThread)
        {
            calls = 0;
            sw = new Stopwatch();
            sw.Start();
            nThreadMax = nThread;
            dispatcherThread.Start();
        }
              



        public void AddRequest(MessageMqtt message)
        {
            Measure();
            /// per aggiungere un nuovo elemento in coda, la blocco prima di modificarla
            lock (listLock)
            {
                queueMessage.Enqueue(message);
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }
            ProcessRequest();
        }

        static void DispatcherThread()
        {
            while (true)
            {
                foreach (KeyValuePair<long, Worker> kvpWorker in mapThread)
                {
                    lock (listLock)
                    {
                        ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                        if (queueMessage.Count == 0)
                        {
                            ///rilascio listLock, riacquistandolo solo dopo essere stato svegliato da una chiamata a Pulse
                            Monitor.Wait(listLock);
                        }

                        MessageMqtt messageDeque = queueMessage.Dequeue();
                        kvpWorker.Value.AddData(messageDeque);

                    }
                }
            }
        }

        public static void InstanceThread()
        {
            Worker w = new Worker();
            mapThread.Add(mapThread.Count + 1, w);
            w.Start();
        }

        public static void ProcessRequest()
        {

            if (mapThread.Count > 0) // se non ho thread nella lista devo avviare il primo
            {
                /// il numero di thread in lista deve essere < o = al numero di richieste/10 (20req/s = 2th , 60 req/s = 6 th, etc...)
                if (requestPerSecond / 10 > mapThread.Count)
                {
                    ///se il numero di th presenti in lista e' minore del numero massimo di th che e' possibile creare
                    ///se aggiunfo un'altro e lo avvio
                    if (mapThread.Count <= nThreadMax)
                    {
                        InstanceThread();
                    }
                }
                else
                {
                    ///se sono disponibili piu' th rispetto al numero di richieste , ne elimino uno
                    ///se il numero di richieste passa da 100 req/s a 20 req/s verra' interrotto un th ad ogni ciclo
                    ///fino a raggiungere il minimo necessario
                    if (mapThread.Count > 1 && requestPerSecond / 10 != mapThread.Count)
                    {
                        Worker w;
                        if (mapThread.TryGetValue(mapThread.Count, out w))
                        {
                            w.Stop();
                            mapThread.Remove(mapThread.Count);
                        }
                    }
                }

            }
            else // avvio il primo thread
            {
                InstanceThread();
            }
            //Console.WriteLine("request_per_second = " + requestPerSecond + " thread_live = " + threads.Count + " message_to_be_processed " + queue.Count);

        }


        public void setNThreadMax(int n)
        {
            nThreadMax = n;
        }

        ///misura il numero di richieste al secondo
        private void Measure()
        {

            calls++;
            if (sw.ElapsedMilliseconds > 1000)
            {
                sw.Stop();
                requestPerSecond = calls;
                calls = 0;
                sw.Restart();

            }
        }

    }
}
