using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Infra.Standard.Caching.Implementations
{
    public class AspNetCache : ICache
    {
        public void Add(string key, object value)
        {
            Add(key, value, DateTime.Now.AddMinutes(10));
        }

        public bool Contains(string key)
        {
            return HttpRuntime.Cache.Get(key) != null;
        }

        public long Count
        {
            get { return HttpContext.Current.Cache.Count; }
        }

        public void Clear()
        {
            IDictionaryEnumerator enumerator = HttpContext.Current.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                HttpContext.Current.Cache.Remove((string)enumerator.Key);
            }
        }

        public object GetData(string key)
        {
            return HttpRuntime.Cache.Get(key);
        }

        public void Remove(string key)
        {
            if (Contains(key))
                HttpRuntime.Cache.Remove(key);
        }

        public object this[string key]
        {
            get { throw new NotImplementedException(); }
        }


        public void Add(string key, object value, DateTime absoluteExipation)
        {
            if (null != value)
                HttpRuntime.Cache.Add(key, value, null, absoluteExipation, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public void Add(string key, object value, TimeSpan slidingExpiration)
        {
            if (null != value)
                HttpRuntime.Cache.Add(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
        }

        public void Add(string key, object value, string dependancyFilePath)
        {
            CacheDependency dep = new CacheDependency(dependancyFilePath);

            HttpRuntime.Cache.Add(key, value, dep, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
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
                var dict = GetData<Dictionary<string, object>>(hashKey);
                return dict.Count;
            }

            return 0;
        }

        public void UpdateInHash<T>(string hashKey, string fieldKey, T value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
