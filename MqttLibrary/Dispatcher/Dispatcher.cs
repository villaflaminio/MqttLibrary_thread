using MqttApp;
using MqttLibrary.Worker;
using MqttSubscriber.model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace MqttSubscriber.Dispatcher
{

    public class Dispatcher
    {

        static private int calls; // numero di chiamate di AddRequest ricevute 
        static private Stopwatch sw;
        static private int requestPerSecond;
        static private int nThreadMax;
        private static Dispatcher _instance;

        static ConcurrentQueue<MessageMqtt> _messageQueueConcurrent = new ConcurrentQueue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        static SemaphoreSlim _messageQueueAvailable = new SemaphoreSlim(0);
        //static Queue<Thread> threads = new Queue<Thread>(); //contiene tutti i thread
        //private static Dictionary<long, Queue<MessageMqtt>> mapMessage = new Dictionary<long, Queue<MessageMqtt>>();
        private static Dictionary<long, Worker> mapThread = new Dictionary<long, Worker>();

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
           
            _messageQueueConcurrent.Enqueue(message);
            _messageQueueAvailable.Release(1);
            ProcessRequest();
        }

        //private static T GetReflectionObject<T>(String type) 
        //{
        //    ///Dichiara l'istanza di classe Assembly per caricare l'assembly di sistema corrente
        //    Assembly executing = Assembly.GetExecutingAssembly();

        //    ///memorizzo tutti i tipi contenuti nell'assembly
        //    Type[] types = executing.GetTypes();
        //    foreach (Type classType in types)
        //    {
        //        ///cerco l'assembly che e' un'istanza dell'interfaccia desiderata 
        //        if (typeof(T).IsAssignableFrom(classType) && classType.IsClass)
        //        {
        //            ///instanzio un'oggetto che e' contenuto dell'assembly di sistema
        //            T instance = (T) Activator.CreateInstance(classType);

        //            ///prendo il metodo getTipo e lo eseguo per vedere il messaggio custom contenuto nella singola istanza
        //            MethodInfo methodInfoOfInstance = classType.GetMethod("getTipo");
        //            String tipo = (String)methodInfoOfInstance.Invoke(instance, null);
        //            ///se il messaggio e' del tipo desiderato allora ho trovato la classe desiderata 
        //            if (tipo.Equals(type))
        //            {

        //                ///dopo aver controllato che l'oggetto implementa l'interfaccia posso eseguire il cast
        //                // IWorker workerRom = (WorkerRom)instance;
        //                //Console.WriteLine(tipo);
        //                return instance;
        //            }
        //        }
        //    }
        //    throw new Exception("Non trovato");
        //}


        //private static void DispatchSinlgeMessage(MessageMqtt messageDeque)
        //{
        //    string[] messageSplit = messageDeque.Payload.Split(",");

        //    Type typeIWorker = typeof(IWorker);
        //    Type typeISpecialWorker = typeof(ISpecialWorker);

        //    //Type workerType = typeof(WorkerA);
        //    //Type typeIWorker = workerType.GetInterface("IWorker");

        //    //Type specialWorkerType = typeof(SpecialWorker);
        //    //Type typeISpecialWorker = specialWorkerType.GetInterface("ISpecialWorker");
        //    switch (messageSplit[0])
        //    {
        //        case "IWorker":
        //            IWorker worker = GetReflectionObject<IWorker>(messageSplit[1]);
        //            worker.Start(messageDeque);
        //            break;
        //        case "ISpecialWorker":
        //            ISpecialWorker specialWorker = GetReflectionObject<ISpecialWorker>("special_worker");
        //            specialWorker.Start(messageDeque);
        //            break;

        //    }
        //}



        private static object GetReflectionObject(String type, Type t)
        {
            ///Dichiara l'istanza di classe Assembly per caricare l'assembly di sistema corrente
            Assembly executing = Assembly.GetExecutingAssembly();

            ///memorizzo tutti i tipi contenuti nell'assembly
            Type[] types = executing.GetTypes();
            foreach (Type classType in types)
            {
                ///cerco l'assembly che e' un'istanza dell'interfaccia desiderata 
                if (t.IsAssignableFrom(classType) && classType.IsClass)
                {
                    ///instanzio un'oggetto che e' contenuto dell'assembly di sistema
                    object instance = Activator.CreateInstance(classType);

                    ///prendo il metodo getTipo e lo eseguo per vedere il messaggio custom contenuto nella singola istanza
                    MethodInfo methodInfoOfInstance = classType.GetMethod("getTipo");
                    String tipo = (String)methodInfoOfInstance.Invoke(instance, null);
                    ///se il messaggio e' del tipo desiderato allora ho trovato la classe desiderata 
                    if (tipo.Equals(type))
                    {
                        return instance;
                        ///dopo aver controllato che l'oggetto implementa l'interfaccia posso eseguire il cast
                       // IWorker workerRom = (WorkerRom)instance;
                        //Console.WriteLine(tipo);
                    }
                }
            }
            return null;
        }

        private static void DispatchSinlgeMessage(MessageMqtt messageDeque)
        {
            string[] messageSplit = messageDeque.Payload.Split(",");

            Type workerType = typeof(WorkerA);

            Type typeIWorker = workerType.GetInterface("IWorker");

            Type specialWorkerType = typeof(SpecialWorker);
            Type typeISpecialWorker = specialWorkerType.GetInterface("ISpecialWorker");
            switch (messageSplit[0])
            {
                case "IWorker":
                    switch (messageSplit[1])
                    {
                        case "worker_a":
                            IWorker workerA = (WorkerA)GetReflectionObject(messageSplit[1], typeIWorker);
                            workerA.Start(messageDeque);
                            break;
                        case "worker_b":
                            IWorker workerB = (WorkerB)GetReflectionObject(messageSplit[1], typeIWorker);
                            workerB.Start(messageDeque);

                            break;

                    }
                    break;
                case "ISpecialWorker":
                    ISpecialWorker specialWorker = (SpecialWorker)GetReflectionObject("special_worker", typeISpecialWorker);
                    specialWorker.Start(messageDeque);

                    break;

            }
        }



        static void DispatcherThread()
        {
            while (true)
            {
                _messageQueueAvailable.Wait();

                if (mapThread.Count > 0)
                {
                    //foreach (KeyValuePair<long, > kvpWorker in mapThread)
                    // foreach (Worker kvpWorker in mapThread.Values) Collection was modified; enumeration operation may not execute.'
                    lock (mapThread)
                    {
                        for (int x = 0; x < mapThread.Count; x++)
                        {   
                            ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                            if (_messageQueueConcurrent.Count == 0)
                                {
                                ///rilascio listLock, riacquistandolo solo dopo essere stato svegliato da una chiamata a Pulse
                                //Monitor.Wait(listLock);                              

                                    break;
                                }

                                MessageMqtt messageDeque;
                                _messageQueueConcurrent.TryDequeue(out messageDeque);
                                string[] messageSplit = messageDeque.Payload.Split(",");

                                if (messageSplit[0].Equals("worker")){
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
                                else
                                {
                                    DispatchSinlgeMessage(messageDeque);
                                }
                            }
                        }
                    }
                
            }
        }

        private static void InstanceThread()
        {
            Worker w = new Worker(mapThread.Count + 1);

            ///register with an event
            w.ProcessKilled += worker_killed;
            Thread t = new Thread(w.Start);
            lock (mapThread)
            {
                mapThread.TryAdd(mapThread.Count + 1, w);
            }
            t.Start();
        }
        /// <summary>
        /// Questo metodo viene richiamato tramite evento di arresto del worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="worker_code"></param>
        public static void worker_killed(object sender, int worker_code)
        {
            lock (mapThread)
            {
                Worker w;
                if (mapThread.Remove(worker_code, out w))
                {
                    Console.WriteLine("Thread suicided");
                    ///comunico che ci sono nuovi elementi in coda           
                    Monitor.Pulse(mapThread); ///sblocco la lista
                }
            }
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
               // Console.WriteLine("request_per_second = " + requestPerSecond + " thread_live = " + mapThread.Count + " message_to_be_processed " + messageQueue.Count);
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
