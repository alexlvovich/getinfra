using GetInfra.Standard.Caching.Implementations.Config.Redis;
using GetInfra.Standard.Caching.Model;
using System.Collections.Generic;

namespace GetInfra.Standard.Caching.Extentions
{
    public static class RedisConfigurationSectionExtentions
    {
        public static GenericConfig ToGenericConfig(this RedisConfigurationSection section)
        {
            if (null != section)
            {
                var genericConfig = new GenericConfig();
                genericConfig.Endpoints = new List<string>();
                foreach (var e in section.Endpoints)
                {
                    var endPoint = (RedisEndpointElement)e;
                    genericConfig.Endpoints.Add(endPoint.Host);
                }

                return genericConfig;
            }

            return null;
        }
    }
}
