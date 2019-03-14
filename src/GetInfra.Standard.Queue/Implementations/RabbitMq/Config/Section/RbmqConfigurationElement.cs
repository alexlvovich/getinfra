using Newtonsoft.Json;
using System;
using System.Configuration;
using RMQExchangeType = RabbitMQ.Client.ExchangeType;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section
{
    public class RbmqConfigurationElement : ConfigurationElement
    {
        [JsonProperty("host")]
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [JsonProperty("username")]
        [ConfigurationProperty("username", IsRequired = true)]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("exchange", IsRequired = true)]
        public string Exchange
        {
            get { return (string)this["exchange"]; }
            set { this["exchange"] = value; }
        }

        [ConfigurationProperty("queue", IsRequired = false)]
        public string Queue
        {
            get { return (string)this["queue"]; }
            set { this["queue"] = value; }
        }

        [ConfigurationProperty("routingKey", IsRequired = false)]
        public string RoutingKey
        {
            get { return (string)this["routingKey"]; }
            set { this["routingKey"] = value; }
        }

        /// <summary>
        /// PrefetchCount param
        /// </summary>
        [ConfigurationProperty("qos", IsRequired = false, DefaultValue = (ushort)1)]
        public ushort QoS
        {
            get { return (ushort)this["qos"]; }
            set { this["qos"] = value; }
        }

        [ConfigurationProperty("exchangeType", IsRequired = true, DefaultValue = RMQExchangeType.Direct)]
        [RegexStringValidator("(topic|headers|direct|fanout)")]
        public string ExchangeType
        {
            get { return (string)this["exchangeType"]; }
            set { this["exchangeType"] = value; }
        }

        [ConfigurationProperty("isDurable", IsRequired = false)]
        public bool IsDurable
        {
            get { return (bool)this["isDurable"]; }
            set { this["isDurable"] = value; }
        }

        [ConfigurationProperty("autoDelete", IsRequired = false)]
        public bool AutoDelete
        {
            get { return (bool)this["autoDelete"]; }
            set { this["autoDelete"] = value; }
        }

        [ConfigurationProperty("messageLimit", IsRequired = false)]
        public ushort MessageLimit
        {
            get { return (ushort)this["messageLimit"]; }
            set { this["messageLimit"] = value; }
        }

        [ConfigurationProperty("vhost", IsRequired = false)]
        public string Vhost
        {
            get { return (string)this["vhost"]; }
            set { this["vhost"] = value; }
        }

        [ConfigurationProperty("bind", IsRequired = false, DefaultValue = false)]
        public bool Bind
        {
            get { return (bool)this["bind"]; }
            set { this["bind"] = value; }
        }

        [ConfigurationProperty("deadLetters", IsRequired = false)]
        public bool DeadLetters
        {
            get { return (bool)this["deadLetters"]; }
            set { this["deadLetters"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = false, DefaultValue = 5672)]
        public int Port
        {
            get { return Convert.ToInt32(this["port"]); }
            set { this["port"] = value; }
        }

        /// <summary>
        /// name for configuration instance
        /// </summary>
        [ConfigurationProperty("name", IsRequired = false)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }
}
