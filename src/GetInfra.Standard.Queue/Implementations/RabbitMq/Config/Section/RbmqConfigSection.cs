using System.Configuration;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section
{
    public class RbmqConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("queueConfigs", IsRequired = true, IsDefaultCollection = true)]
        public RbmqConfigurationElementCollection Queues
        {
            get { return (RbmqConfigurationElementCollection)this["queueConfigs"]; }
            set { this["queueConfigs"] = value; }
        }
    }
}
