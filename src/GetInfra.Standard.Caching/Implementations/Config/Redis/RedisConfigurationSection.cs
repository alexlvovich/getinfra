using System.Collections.Generic;
using System.Configuration;

namespace GetInfra.Standard.Caching.Implementations.Config.Redis
{
    public class RedisConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("endpoints")]
        [ConfigurationCollection(typeof(RedisEndpointElementCollection), AddItemName = "add")]
        public RedisEndpointElementCollection Endpoints
        {
            get
            {
                return (RedisEndpointElementCollection)this["endpoints"];
            }
            set
            { this["endpoint"] = value; }
        }


     
    }
}
