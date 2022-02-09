using MqttApp;
using MqttSubscriber.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttApp
{
    internal class SpecialWorker : ISpecialWorker
    {
        public String getTipo()
        {
            return ("special_worker");
        }

        public void Start(MessageMqtt message)
        {
            Console.WriteLine("----- BY SPECIAL WORKER A -> " + message.ToString()+ "----------");

        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
