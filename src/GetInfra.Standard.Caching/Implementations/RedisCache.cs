using GetInfra.Standard.Caching.Implementations.Config.Redis;
using GetInfra.Standard.Caching.Model;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GetInfra.Standard.Caching.Implementations
{
    public class RedisCache : ICache
    {
        private static ConnectionMultiplexer redis;
        private readonly IDatabase db;
        private readonly JsonSerializerSettings _serializationSettings;
        private readonly GenericConfig _config;
        public long Count
        {
            get
            {
              

                var configurationOptions = new ConfigurationOptions
                {
                    SyncTimeout = int.MaxValue,
                };


                foreach (var e in _config.Endpoints)
                {
                    configurationOptions.EndPoints.Add(e);
                }

                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);


                var server = redis.GetServer(_config.Endpoints[0]);
                var keysCount = server.Keys().Count();
                return keysCount;
              
            }
        }

        public RedisCache(GenericConfig config)
        {
            // init config
            _config = config;
            
            var configurationOptions = new ConfigurationOptions
            {
                SyncTimeout = int.MaxValue,
            };


            foreach (var e in _config.Endpoints)
            {
                configurationOptions.EndPoints.Add(e);
            }

            redis = ConnectionMultiplexer.Connect(configurationOptions);

            db = redis.GetDatabase();

            _serializationSettings = new JsonSerializerSettings
            {
                //PreserveReferencesHandling = PreserveReferencesHandling.Objects
                Formatting = Formatting.Indented
            };
        }

        public RedisCache(JsonSerializerSettings serializationSettings, GenericConfig config) : this(config)
        {
            this._serializationSettings = serializationSettings;
        }


        public object this[string key] => db.StringGet(key);

     

        public void Add(string key, object value)
        {
            if (value is string)
                db.StringSet(key, value.ToString());
            else
            {
                db.StringSet(key, JsonConvert.SerializeObject(value, Formatting.Indented, _serializationSettings));
            }

        }

        public void Add(string key, object value, DateTime absoluteExipation)
        {
            if (value is string)
            {
                db.StringSet(key, value.ToString());
               
            }
            else
            {
                db.StringSet(key, JsonConvert.SerializeObject(value, Formatting.Indented, _serializationSettings));
            }
            db.KeyExpire(key, absoluteExipation, flags: CommandFlags.FireAndForget);
        }

        public void Add(string key, object value, TimeSpan slidingExpiration)
        {
            if (value is string)
            {
                db.StringSet(key, value.ToString(), slidingExpiration);
              
            }
            else
            {
                db.StringSet(key, JsonConvert.SerializeObject(value, Formatting.Indented, _serializationSettings), slidingExpiration);
            }
            db.KeyExpire(key, slidingExpiration, CommandFlags.FireAndForget);


        }

        public void Add(string key, object value, string dependancyFilePath)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string key)
        {
            return db.KeyExists(key);
        }

        public void Clear()
        {
            var config = (RedisConfigurationSection)ConfigurationManager.GetSection("redis");

            var configurationOptions = new ConfigurationOptions
            {
                SyncTimeout = int.MaxValue,
                AllowAdmin = true
            };


            foreach (var e in config.Endpoints)
            {
                var endpoint = (RedisEndpointElement)e;
                configurationOptions.EndPoints.Add(endpoint.Host);
            }

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);

            foreach (var e in configurationOptions.EndPoints)
            {
                var server = redis.GetServer(e);
                server.FlushAllDatabases();
            }
        }

        public object GetData(string key)
        {
            var item = db.StringGet(key);
            if(item.HasValue)
                return item as object;
            return null;
        }

        public T GetData<T>(string key)
        {
            var item = GetData(key);
            if (item != null)
            {
                // check if object
                if (!typeof(T).IsPrimitive && typeof(T) != typeof(string))
                {
                    return JsonConvert.DeserializeObject<T>(item.ToString(), _serializationSettings);
                }
                else
                {
                    return (T)Convert.ChangeType(item, typeof(T)); ;
                }
            }

            return default(T);
        }

        public void Remove(string key)
        {
            db.KeyDelete(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashid"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddToHash<T>(string hashKey, Dictionary<string, T> dict)
        {
            var hashList = new List<HashEntry>();// dict.Select(pair => new HashEntry((RedisValue)pair.Key, pair.Value)).ToArray();

            foreach (var k in dict.Keys)
            {
                hashList.Add(new HashEntry(k, JsonConvert.SerializeObject(dict[k], _serializationSettings)));
            }

            db.HashSet(hashKey, hashList.ToArray());
        }

        public T GetFromHash<T>(string hashKey, string fieldKey)
        {
            var item = db.HashGet(hashKey, fieldKey);
            if (item.HasValue)
            {
                // check if object
                if (!typeof(T).IsPrimitive && typeof(T) != typeof(string))
                {
                    return JsonConvert.DeserializeObject<T>(item.ToString(), _serializationSettings);
                }
                else
                {
                    return (T)Convert.ChangeType(item, typeof(T)); ;
                }
            }

            return default(T);

            // questions to resolve:

            // update
            // use different DB

        }

        public async Task<List<T>> GetAllFromHash<T>(string hashKey)
        {
            var list = new List<T>();
            var items = await db.HashGetAllAsync(hashKey);
            if (items.Length > 0)
            {
                foreach (var item in items)
                {
                    // check if object
                    if (!typeof(T).IsPrimitive && typeof(T) != typeof(string))
                    {
                        list.Add(JsonConvert.DeserializeObject<T>(item.Value, _serializationSettings));
                    }
                    else
                    {
                        list.Add((T)Convert.ChangeType(item, typeof(T)));
                    }
                }
                
            }

            return list;

            // questions to resolve:

            // update
            // use different DB

        }

        public bool ExistsInHash<T>(string hashKey, string fieldKey)
        {
            return db.HashExists(hashKey, fieldKey);
        }

        public bool DeleteFromHash<T>(string hashKey, string fieldKey)
        {
            return db.HashDelete(hashKey, fieldKey);
        }

        public long HashCount(string hashKey)
        {
            return db.HashLength(hashKey);
        }

        public void UpdateInHash<T>(string hashKey, string fieldKey, T value)
        {
            var serialized = JsonConvert.SerializeObject(value, _serializationSettings);
            db.HashSet(hashKey, (RedisValue)fieldKey, (RedisValue)serialized);
        }
    }
}
