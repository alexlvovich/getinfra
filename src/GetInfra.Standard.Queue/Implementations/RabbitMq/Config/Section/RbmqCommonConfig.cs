using System.Configuration;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section
{
    public class MsgQueues
    {
        public static MsgQueuesSection _Config = ConfigurationManager.GetSection("MsgQueues") as MsgQueuesSection;
        public static RbmqConfigurationElementCollection GetQueues()
        {
            return _Config.Queues;
        }
    }

    public class MsgQueuesSection : ConfigurationSection
    {
        //Decorate the property with the tag for your collection.
        [ConfigurationProperty("Queues")]
        public RbmqConfigurationElementCollection Queues
        {
            get { return (RbmqConfigurationElementCollection)this["Queues"]; }
        }
    }
  
}
