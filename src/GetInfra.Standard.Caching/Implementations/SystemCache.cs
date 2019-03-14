using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GetInfra.Standard.Caching.Implementations
{
    public class SystemCache : ICache
    {
        private static object _lock = new Object();
        private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public long Count => _cache.Count;

        public object this[string key]
        {
            get
            {
                lock (_lock)
                {
                    return _cache.Get(key);
                }
            }
        }

        public void Add(string key, object value)
        {
            Add(key, value, DateTime.Now.AddMinutes(5));
        }

        public void Add(string key, object value, DateTime absoluteExipation)
        {
            lock(_lock)
            {
                _cache.Set(key, value, absoluteExipation);
            }
            
        }

        public void Add(string key, object value, TimeSpan slidingExpiration)
        {
            lock (_lock)
            {
                _cache.Set(key, value, slidingExpiration);
            }
        }

        public void Add(string key, object value, string dependancyFilePath)
        {
            
        }

        public bool Contains(string key)
        {
            var o = new object();
            if (_cache.TryGetValue(key, out o))
            {
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _cache.Dispose();

            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public object GetData(string key)
        {
            lock (_lock)
            {
                return _cache.Get(key);
            }
        }

        public T GetData<T>(string key)
        {
            var item = GetData(key);
            if (item != null)
            {
                return (T)Convert.ChangeType(item, typeof(T)); ;
            }

            return default(T);
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
            }
        }

        #region HASHING

        public void AddToHash<T>(string hashKey, Dictionary<string, T> dict)
        {
            Add(hashKey, dict);
        }

        public T GetFromHash<T>(string hashKey, string fieldKey)
        {
            if (Contains(hashKey))
            {
                var dict = GetData<Dictionary<string, T>>(hashKey);
                if (dict.ContainsKey(fieldKey))
                {
                    return (T)Convert.ChangeType(dict[fieldKey], typeof(T)); ;
                }
            }
            return default(T);
        }

        public bool ExistsInHash<T>(string hashKey, string fieldKey)
        {
            if (Contains(hashKey))
            {
                var dict = GetData<Dictionary<string, T>>(hashKey);
                return dict.ContainsKey(fieldKey);
            }
            return false;
        }

        public bool DeleteFromHash<T>(string hashKey, string fieldKey)
        {
            if (Contains(hashKey))
            {
                var dict = GetData<Dictionary<string, T>>(hashKey);
                if (dict.ContainsKey(fieldKey))
                {
                    dict.Remove(fieldKey);
                    Add(hashKey, dict);
                    return true;
                }
            }

            return false;
        }

        public long HashCount(string hashKey)
        {
            if (Contains(hashKey))
            {
                var dict = GetData(hashKey);
                string s = JsonConvert.SerializeObject(dict);
                var d = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(s);
                return d.Count;
            }

            return 0;
        }

        public void UpdateInHash<T>(string hashKey, string fieldKey, T value)
        {
            if (!Contains(hashKey))
            {
                var dict = new Dictionary<string, T>();
                dict.Add(fieldKey, value);
                Add(hashKey, dict);
            }
            else
            {
                var dict = GetData<Dictionary<string, T>>(hashKey);
                if (dict.ContainsKey(fieldKey))
                    dict[fieldKey] = value;
                else
                    dict.Add(fieldKey, value);

                Add(hashKey, dict);
            }
                
        }

        public async Task<List<T>> GetAllFromHash<T>(string hashKey)
        {
            if (Contains(hashKey))
            {
                var dict = GetData<Dictionary<string, T>>(hashKey);
                return await Task.FromResult(dict.Values.ToList());
            }

            return null;
        }
        #endregion
    }
}
