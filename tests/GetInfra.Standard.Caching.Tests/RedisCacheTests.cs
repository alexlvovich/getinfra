using GetInfra.Standard.Caching.Extentions;
using GetInfra.Standard.Caching.Implementations;
using GetInfra.Standard.Caching.Implementations.Config.Redis;
using GetInfra.Standard.Caching.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Caching.Tests
{
    public class RedisCacheTests
    {
        public RedisCacheTests()
        {
            Init();
        }
        private GenericConfig _config;

        
        public void Init()
        {
            var config = (RedisConfigurationSection)ConfigurationManager.GetSection("redis");
            _config = config.ToGenericConfig();
        }
        [Fact]
        public void RedisCache_TestSliding()
        {
            var key = "slidingCacheKey";

            var cache = new RedisCache(_config);

            var dummy = new DummyObject() { Id = 1 };

            cache.Add(key, dummy, TimeSpan.FromSeconds(2));

            Thread.Sleep(500);

            for (int i = 0; i < 3; i++)
            {
                var o = cache.GetData<DummyObject>(key);

                Assert.True(o != null, string.Format("failed on {0} iteration", i));
                Thread.Sleep(500);
            }

            Thread.Sleep(2001);

            var ob = cache.GetData(key);

            Assert.True(ob == null);


        }



        [Fact]
        public void RedisCache_StringValueTest()
        {
            var key = "test-string";

            var cache = new RedisCache(_config);

//            var dummy = new DummyObject() { Id = 1 };

            cache.Add(key, "La la la");

            Thread.Sleep(500);

            
            var o = cache.GetData(key);

            Assert.True(o != null, "failed on {0} iteration");
            Assert.Equal(o, "La la la");
            
            string str = cache.GetData<string>(key);

            Assert.True(str != null, "failed on {0} iteration");
            Assert.Equal(str, "La la la");


        }


        [Fact]
        public void RedisCache_TestExpirationDate()
        {
            var key = "absCacheKey";

            var cache = new RedisCache(_config);

            var dummy = new DummyObject() { Id = 2 };

            cache.Add(key, dummy, DateTime.UtcNow.AddSeconds(5));

            Thread.Sleep(2000);

           
            var o = cache.GetData<DummyObject>(key);

            Assert.True(o != null);
            // Assert.InstanceOf(typeof(DummyObject), o);
            Thread.Sleep(10001);


            o = cache.GetData<DummyObject>(key);
            
            Assert.True(o == null);


        }


        [Fact]
        public void RedisCache_Clear()
        {
            var key = "clearCacheKey";

            var cache = new RedisCache(_config);
          
            long count = cache.Count;

            Random rnd = new Random(DateTime.Now.Millisecond);
            
            for (int i = 1; i <= 10; i++)
            {
                var dummy = new DummyObject() { Id = rnd.Next(10001, 99999), Name = "for test" };
                cache.Add($"item-{i}", dummy);
            }

            var newCount = cache.Count;

            Assert.True(newCount - 10 == count);

            cache.Clear();

            Task.Delay(500);

            Assert.True(cache.Count == 0);
        }

      
        [Fact]
        public void RedisCache_Hash()
        {
            var key = "myhash";

            var cache = new RedisCache(_config);

            long count = cache.Count;

            Random rnd = new Random(DateTime.Now.Millisecond);

            var dict = new Dictionary<string, DummyObject>();

            for (int i = 1; i <= 1000; i++)
            {
                var dummy = new DummyObject() { Id = rnd.Next(10001, 99999), Name = "for test" };
                if(!dict.ContainsKey(dummy.Id.ToString()))
                    dict.Add(dummy.Id.ToString(), dummy);
            }


            cache.AddToHash<DummyObject>(key, dict);
            // get random

            List<string> keys = new List<string>(dict.Keys);
            var k = keys[rnd.Next(keys.Count)];
            var obj = dict[k];

            Assert.True(cache.ExistsInHash<DummyObject>(key, obj.Id.ToString()));

            var cachedObj = cache.GetFromHash<DummyObject>(key, obj.Id.ToString());

            Assert.Equal(obj, cachedObj);
            
        }

       
        [Fact]
        public void RedisCache_HashUpdate()
        {
            var key = "myhash";

            var cache = new RedisCache(_config);
          
            Random rnd = new Random(DateTime.Now.Millisecond);

            var dict = new Dictionary<string, DummyObject>();

            for (int i = 1; i <= 1000; i++)
            {
                var dummy = new DummyObject() { Id = rnd.Next(10001, 99999), Name = "for test" };
                if (!dict.ContainsKey(dummy.Id.ToString()))
                    dict.Add(dummy.Id.ToString(), dummy);
            }


            cache.AddToHash<DummyObject>(key, dict);

            var count = cache.HashCount(key);

            // get random

            List<string> keys = new List<string>(dict.Keys);
            var k = keys[rnd.Next(keys.Count)];
            var obj = dict[k];

            Assert.True(cache.ExistsInHash<DummyObject>(key, obj.Id.ToString()));

            var cachedObj = cache.GetFromHash<DummyObject>(key, obj.Id.ToString());

            // update
            cachedObj.Name = "update";

            // update
            cache.UpdateInHash<DummyObject>(key, obj.Id.ToString(), cachedObj);

            var updatedObj = cache.GetFromHash<DummyObject>(key, obj.Id.ToString());

            Assert.True(updatedObj.Name == "update");

            var newcount = cache.HashCount(key);

            Assert.Equal(count, newcount);
           
        }

        [Fact]
        public async Task GetAllFromHash()
        {
            var key = "allhash";

            var cache = new RedisCache(_config);

            cache.UpdateInHash<DummyObject>(key, "item1", new DummyObject() { Id = 1, Name = "test1" });

            cache.UpdateInHash<DummyObject>(key, "item2", new DummyObject() { Id = 2, Name = "test2" });

            var list = await cache.GetAllFromHash<DummyObject>(key);

            Assert.True(list.Count == 2);
        }

        //[TearDown]
        public void Dispose()
        {
            var cache = new RedisCache(_config);
            cache.Clear();
        }

    }

    public class DATString
    {
        public int DATStringId { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public List<Equipment> Equipments { get; set; }
    }

    public class Equipment
    {
        public HashSet<DATString> DATStrings { get; set; }
    }

}
