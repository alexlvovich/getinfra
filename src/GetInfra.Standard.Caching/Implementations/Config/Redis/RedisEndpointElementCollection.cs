using System;
using System.Configuration;

namespace GetInfra.Standard.Caching.Implementations.Config.Redis
{
    public class RedisEndpointElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RedisEndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RedisEndpointElement)element);
        }

        public RedisEndpointElement this[int index]
        {
            get
            {
                return (RedisEndpointElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}
