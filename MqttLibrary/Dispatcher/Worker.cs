using MqttSubscriber.model;
using MqttSubscriber.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLibrary.Dispatcher
{
    public class Worker
    {

        private static bool flag = false;
        static Queue<MessageMqtt> messageQueue = new Queue<MessageMqtt>(); //contiene la coda con i messaggi che vengono ricevuti
        private static Repository db = new Repository();

        private void SaveMessage(MessageMqtt messageDeque)
        {
            messageDeque.Id = null; // l'id deve essere generato automaticamente da repository
            db.Messages.Add(messageDeque);

            Console.WriteLine("Saving -> " + messageDeque.ToString());

            db.SaveChanges();
        }
        public void Start()
        {
            while (!flag)
            {
                try
                {

                    ///Se la coda è vuota, devo attendere che venga aggiunto un elemento
                    while (messageQueue.Count != 0)
                    {
                        MessageMqtt messageDeque = messageQueue.Dequeue();
                        SaveMessage(messageDeque);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }


            }

            ///devo finire di svuotare la coda prima di arrestare il Thread
            while (messageQueue.Count > 0)
            {
                MessageMqtt messageDeque = messageQueue.Dequeue();
                SaveMessage(messageDeque);
                // do something with queue value here
            }

        }




        public void AddData(MessageMqtt m)
        {
            messageQueue.Enqueue(m);
        }

        public void Stop()
        {
            flag = true;
        }
    }
}
