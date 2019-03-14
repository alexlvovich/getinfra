using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GetInfra.Standard.Caching
{
    public interface ICache
    {
        /// <summary>
        /// adds new object to cache with default expiration
        /// <param name="key">Identifier for this CacheItem</param>
        /// <param name="value">Value to be stored in cache. May be null.</param>
        /// <exception cref="ArgumentNullException">Provided key is null</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string</exception>
        /// <remarks></remarks>
        void Add(string key, object value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExipation"></param>
        void Add(string key, object value, DateTime absoluteExipation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingExpiration"></param>
        void Add(string key, object value, TimeSpan slidingExpiration);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dependancyFilePath"></param>
        void Add(string key, object value, string dependancyFilePath);

        /// <summary>
        /// Returns true if key refers to item current stored in cache
        /// </summary>
        /// <param name="key">Key of item to check for</param>
        /// <returns>True if item referenced by key is in the cache</returns>
        bool Contains(string key);

        /// <summary>
        /// Returns the number of items currently in the cache.
        /// </summary>
        //int Count { get; }

        /// <summary>
        /// Removes all items from the cache. If an error occurs during the removal, the cache is left unchanged.
        /// </summary>
        /// <remarks>The CacheManager can be configured to use different storage mechanisms in which to store the CacheItems.
        /// Each of these storage mechanisms can throw exceptions particular to their own implementations.</remarks>
        //void Flush();

        /// <summary>
        /// Returns the value associated with the given key.
        /// </summary>
        /// <param name="key">Key of item to return from cache.</param>
        /// <returns>Value stored in cache</returns>
        /// <exception cref="ArgumentNullException">Provided key is null</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string</exception>
        /// <remarks></remarks>
        object GetData(string key);


        /// <summary>
        /// REturns generic value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>Converted value stored in cache</returns>
        /// <exception cref="ArgumentNullException">Provided key is null</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string</exception>
        /// <remarks></remarks>
        T GetData<T>(string key);


        /// <summary>
        /// Removes the given item from the cache. If no item exists with that key, this method does nothing.
        /// </summary>
        /// <param name="key">Key of item to remove from cache.</param>
        /// <exception cref="ArgumentNullException">Provided key is null</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string</exception>
        /// <remarks></remarks>
        void Remove(string key);

        /// <summary>
        /// Returns the item identified by the provided key
        /// </summary>
        /// <param name="key">Key to retrieve from cache</param>
        /// <exception cref="ArgumentNullException">Provided key is null</exception>
        /// <exception cref="ArgumentException">Provided key is an empty string</exception>
        /// <remarks></remarks>
        object this[string key] { get; }

        /// <summary>
        /// 
        /// </summary>
        long Count { get; }

        /// <summary>
        /// 
        /// </summary>
        void Clear();

        /// <summary>
        /// adds objects to cached dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dict"></param>
        void AddToHash<T>(string hashKey, Dictionary<string, T> dict);

        /// <summary>
        /// get object from cached dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashKey"></param>
        /// <param name="fieldKey"></param>
        /// <returns></returns>
        T GetFromHash<T>(string hashKey, string fieldKey);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashKey"></param>
        /// <param name="fieldKey"></param>
        /// <returns></returns>
        bool ExistsInHash<T>(string hashKey, string fieldKey);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashKey"></param>
        /// <param name="fieldKey"></param>
        /// <returns></returns>
        bool DeleteFromHash<T>(string hashKey, string fieldKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hashKey"></param>
        /// <returns></returns>
        long HashCount(string hashKey);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashKey">group cache key</param>
        /// <param name="fieldKey">specific field cahce key</param>
        /// <param name="value">value</param>
        void UpdateInHash<T>(string hashKey, string fieldKey, T value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashKey"></param>
        /// <returns></returns>
        Task<List<T>> GetAllFromHash<T>(string hashKey);
    }
}
