using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using System;

namespace GetInfra.Standard.Queue.Model
{
    public class QueueSettings
    {
        public QueueSettings(RbmqConfigurationElement s)
        {
            Host = s.Host;
            Username = s.Username;
            Password = s.Password;
            Port = s.Port;
            AutoDelete = s.AutoDelete;
            Bind = s.Bind;
            DeadLetters = s.DeadLetters;
            Exchange = s.Exchange;
            ExchangeType = s.ExchangeType;
            RoutingKey = s.RoutingKey;
            QoS = s.QoS;
            Queue = s.Queue;
            MessageLimit = s.MessageLimit;
            IsDurable = s.IsDurable;
            Name = s.Name;
        }

        public string Host { get; set; }
        
        public string Username { get; set; }
        public string Password { get; set; }

        public string Exchange { get; set; }

        public string Queue { get; set; }

        public string RoutingKey { get; set; }
        
        public ushort QoS { get; set; }
        
        public string ExchangeType { get; set; }

        public bool IsDurable { get; set; }

        public bool AutoDelete { get; set; }

        public ushort MessageLimit { get; set; }

        public string Vhost { get; set; }

        public bool Bind { get; set; }

        public bool DeadLetters { get; set; }

        public int Port { get; set; }
        
        public string Name { get; set; }

        public bool Exclusive { get; set; }

        public bool GeneratedQueueName { get; set; }

        public void GenerateNewQueueName(bool isPublisher)
        {
            string dir = isPublisher ? "pub" : "con";
            string computername = System.Environment.MachineName;
            Random rnd = new Random(DateTime.Now.Millisecond);
            string newQueueName = $"{this.Exchange}.{computername}.{dir}-{rnd.Next(12345678, 99999999)}";
            this.Queue = newQueueName;
            this.Exclusive = true;
        }
    }
}
