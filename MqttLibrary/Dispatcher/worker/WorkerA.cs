using MqttSubscriber.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttApp
{
    internal class WorkerA : IWorker
    {
        public WorkerA()
        {
        }

        public String getTipo()
        {
            return("worker_a");
        }

        public void Start(MessageMqtt message)
        {
            Console.WriteLine("BY WORKER A -> " + message.ToString());

        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        
    }
}
