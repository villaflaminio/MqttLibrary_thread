using MqttSubscriber.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttApp
{
    internal interface ISpecialWorker
    {
        public void Start(MessageMqtt message);
            public void Stop();
        public String getTipo();

    }
}
