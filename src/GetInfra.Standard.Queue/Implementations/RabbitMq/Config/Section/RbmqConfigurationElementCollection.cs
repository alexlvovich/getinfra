using System.Configuration;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section
{
    [ConfigurationCollection(typeof(RbmqConfigurationElement))]
    public class RbmqConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RbmqConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RbmqConfigurationElement)element).Name;
        }

        public RbmqConfigurationElement this[int index]
        {
            get { return (RbmqConfigurationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public new RbmqConfigurationElement this[string key]
        {
            get { return base.BaseGet(key) as RbmqConfigurationElement; }
        }
    }
}
