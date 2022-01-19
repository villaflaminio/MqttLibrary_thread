using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttSubscriber.model
{
    public class MessageMqtt
    {
        public int? Id { get; set; }
        public string Payload { get; set; }
        public string Topic { get; set; }
        public DateTime TimeStamp { get; set; }

        public MessageMqtt(string payload, string topic, DateTime timeStamp)
        {
            Payload = payload;
            Topic = topic;
            TimeStamp = timeStamp;
        }

        

        public override string ToString()
        {
            return "Id : " + Id + " Payload : " + Payload + " Topic : " + Topic + " TimeStamp : " + TimeStamp;
        }
    }


}
