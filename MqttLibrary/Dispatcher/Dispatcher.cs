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
        private static Repository db = new Repository();

        static private int calls; // numero di chiamate di AddRequest ricevute 
        static private Stopwatch sw;
        static private int requestPerSecond;


        static private int nThreadMax;
        static private bool connected;


        static Queue<MessageMqtt> queue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        static Queue<Thread> threads = new Queue<Thread>(); //contiene tutti i thread

        Dictionary<int, Queue<MessageMqtt>> mapMessage = new Dictionary<int, Queue<MessageMqtt>>();
        Dictionary<int, Thread> mapThread = new Dictionary<int, Thread>();
        

        readonly static object listLock = new object();


        public Dispatcher(int nThread)
        {
            calls = 0;
            sw = new Stopwatch();
            sw.Start();
            nThreadMax = nThread;
            connected = true;

        }



        void dispatch() { 
        
            
        
        
        
        }



        public void AddRequest(MessageMqtt message)
        {
            Measure();


            /// per aggiungere un nuovo elemento in coda, la blocco prima di modificarla
            lock (listLock)
            {
                queue.Enqueue(message);
                ///comunico che ci sono nuovi elementi in coda           
                Monitor.Pulse(listLock); ///sblocco la lista
            }
            ProcessRequest();
        }

        public static void ProcessRequest()
        {

            if (threads.Count > 0) // se non ho thread nella lista devo avviare il primo
            {
                /// il numero di thread in lista deve essere < o = al numero di richieste/10 (20req/s = 2th , 60 req/s = 6 th, etc...)
                if (requestPerSecond / 10 > threads.Count)
                {
                    ///se il numero di th presenti in lista e' minore del numero massimo di th che e' possibile creare
                    ///se aggiunfo un'altro e lo avvio
                    if (threads.Count <= nThreadMax)
                    {
                        Thread t = new Thread(ProcessRequestThread);
                        threads.Enqueue(t);
                        t.Start();
                    }
                }
                else
                {
                    ///se sono disponibili piu' th rispetto al numero di richieste , ne elimino uno
                    ///se il numero di richieste passa da 100 req/s a 20 req/s verra' interrotto un th ad ogni ciclo
                    ///fino a raggiungere il minimo necessario
                    if (threads.Count > 1 && requestPerSecond / 10 != threads.Count)
                    {
                        Thread t = threads.Dequeue();
                        try
                        {
                            t.Interrupt();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

            }
            else // avvio il primo thread
            {
                Thread t = new Thread(ProcessRequestThread);
                threads.Enqueue(t);
                t.Start();

            }
            //Console.WriteLine("request_per_second = " + requestPerSecond + " thread_live = " + threads.Count + " message_to_be_processed " + queue.Count);

        }

        static void ProcessRequestThread()
        {
            while (true)
            {
                try
                {
                    lock (listLock)
                    {

                        ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                        while (queue.Count == 0)
                        {

                            ///rilascio listLock, riacquistandolo solo dopo essere stato svegliato da una chiamata a Pulse
                            Monitor.Wait(listLock);
                            break;
                        }

                        MessageMqtt messageDeque = queue.Dequeue();
                        messageDeque.Id = null; // l'id deve essere generato automaticamente da repository
                        db.Messages.Add(messageDeque);
                        
                        Console.WriteLine("Saving -> " + messageDeque.ToString());

                        db.SaveChanges();

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }

        public void ComunicateDisconnect()
        {
            connected = false;
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
