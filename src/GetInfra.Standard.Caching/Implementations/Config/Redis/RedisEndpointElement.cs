using System.Configuration;

namespace GetInfra.Standard.Caching.Implementations.Config.Redis
{
    /// <summary>
    /// represents enpoint element of configuration
    /// </summary>
    public class RedisEndpointElement : ConfigurationElement
    {
        [ConfigurationProperty("host", IsRequired = true)]
//        [IntegerValidator(ExcludeRange = false, MaxValue = 24, MinValue = 6)]
        public string Host
        {
            get
            { return (string)this["host"]; }
            set
            { this["host"] = value; }
        }
    }
}
