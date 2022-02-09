using MqttSubscriber.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttApp
{
    internal class WorkerB : IWorker
    {
        public WorkerB()
        {
        }

        public String getTipo()
        {
            return("worker_b");
        }



        public void Start(MessageMqtt message)
        {
            Console.WriteLine("BY WORKER B -> " + message.ToString());

        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
