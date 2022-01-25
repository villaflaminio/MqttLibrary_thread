using MqttLibrary.Worker;
using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private static Dispatcher _instance;



        static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        //static Queue<Thread> threads = new Queue<Thread>(); //contiene tutti i thread
        //private static Dictionary<long, Queue<MessageMqtt>> mapMessage = new Dictionary<long, Queue<MessageMqtt>>();
        private static Dictionary<long, Worker> mapThread = new Dictionary<long, Worker>();


        readonly static object listLock = new object();
        Thread dispatcherThread = new Thread(DispatcherThread);


        private Dispatcher()
        {
            calls = 0;
            sw = new Stopwatch();
            sw.Start();
            dispatcherThread.Start();
        }
        public static Dispatcher GetInstance(int nThread)
        {
            nThreadMax = nThread;
            if (_instance == null)
            {
                _instance = new Dispatcher();
            }
            return _instance;
        }


        public void AddRequest(MessageMqtt message)
        {
            Measure();
            /// per aggiungere un nuovo elemento in coda, la blocco prima di modificarla
            lock (listLock)
            {
                messageQueue.Enqueue(message);
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }
            ProcessRequest();
        }

        static void DispatcherThread()
        {
            while (true)
            {
                if (mapThread.Count > 0)
                {
                    //foreach (KeyValuePair<long, > kvpWorker in mapThread)
                    // foreach (Worker kvpWorker in mapThread.Values) Collection was modified; enumeration operation may not execute.'
                    lock (mapThread)
                    {
                        for (int x = 0; x < mapThread.Count; x++)
                        {

                            lock (listLock)
                            {
                                ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                                if (messageQueue.Count == 0)
                                {
                                    ///rilascio listLock, riacquistandolo solo dopo essere stato svegliato da una chiamata a Pulse
                                    //Monitor.Wait(listLock);
                                    break;
                                }

                                MessageMqtt messageDeque = messageQueue.Dequeue();
                                if (mapThread.ContainsKey(mapThread.ElementAt(x).Key))
                                {
                                    try
                                    {
                                        mapThread.ElementAt(x).Value.AddData(messageDeque);

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void InstanceThread()
        {
            Worker w = new Worker();

            Thread t = new Thread(w.Start);
            lock (mapThread)
            {
                mapThread.TryAdd(mapThread.Count + 1, w);
            }
            t.Start();
        }

        private static void ProcessRequest()
        {
            lock (mapThread)
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
                            if (mapThread.Remove(mapThread.Count, out w))
                            {
                                w.Stop();

                                ///comunico che ci sono nuovi elementi in coda           
                                Monitor.Pulse(mapThread); ///sblocco la lista
                            }
                        }

                    }

                }
                else // avvio il primo thread
                {
                    InstanceThread();
                }
                Console.WriteLine("request_per_second = " + requestPerSecond + " thread_live = " + mapThread.Count + " message_to_be_processed " + messageQueue.Count);
            }
        }


        public void SetNThreadMax(int n)
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
