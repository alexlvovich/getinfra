using GetInfra.Standard.Caching.Implementations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Caching.Tests
{
    public class SystemCacheTests
    {
        [Fact]
        public void SystemCache_TestSliding()
        {
            var key = "slidingCacheKey";

            var cache = new SystemCache();

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
        public void SystemCache_TestExpirationDate()
        {
            var key = "absCacheKey";

            var cache = new SystemCache();

            var dummy = new DummyObject() { Id = 2 };

            cache.Add(key, dummy, DateTime.Now.AddSeconds(5));

            Thread.Sleep(2000);

           
            var o = cache.GetData<DummyObject>(key);

            Assert.True(o != null);
            Assert.IsType(typeof(DummyObject), o);
            Thread.Sleep(3001);


            o = cache.GetData<DummyObject>(key);

            Assert.True(o == null);
        }

        [Fact]
        public void SystemCache_StringValueTest()
        {
            var key = "test-string";

            var cache = new SystemCache();

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
        public void SystemCache_Clear()
        {
            var key = "absCacheKey";

            var cache = new SystemCache();

            long count = cache.Count;

            Random rnd = new Random(DateTime.Now.Millisecond);

            for (int i = 1; i <= 1000; i++)
            {
                var dummy = new DummyObject() { Id = rnd.Next(10001, 99999), Name = "for test" };
                cache.Add($"item-{i}", dummy);
            }

            var newCount = cache.Count;

            Assert.True(newCount - 1000 == count);

            cache.Clear();

            Assert.True(cache.Count == 0);


        }

        [Fact]
        public void SystemCache_TestSelfReferenceObject()
        {
            var key = "selfrefCacheKey";

            var cache = new SystemCache();

            var datString = new DATString()
            {
                DATStringId = 1,
                Name = "test",
                Source = "DAT",
                Type = "SA"
            };
            datString.Equipments = new List<Equipment>();

            datString.Equipments.Add(new Equipment()
            {
                DATStrings = new HashSet<DATString>()
                {
                    datString
                }
            });

            cache.Add(key, datString);

            var ob = cache.GetData<DATString>(key);

            Assert.NotNull(ob);


        }



        [Fact]
        public void SystemCache_Hash()
        {
            var key = "myhash";

            var cache = new SystemCache();

            long count = cache.Count;

            Random rnd = new Random(DateTime.Now.Millisecond);

            var dict = new Dictionary<string, DummyObject>();

            for (int i = 1; i <= 1000; i++)
            {
                var dummy = new DummyObject() { Id = rnd.Next(10001, 99999), Name = "for test" };
                if (!dict.ContainsKey(dummy.Id.ToString()))
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
        public void SystemCache_HashUpdate()
        {
            var key = "myhash";

            var cache = new SystemCache();
            cache.Clear();
            Random rnd = new Random(DateTime.Now.Millisecond);

            var dict = new Dictionary<string, DummyObject>();

            for (int i = 1; i <= 1000; i++)
            {
                var dummy = new DummyObject() { Id = i, Name = "for test" };
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
            // create change set dict
            // update
            cache.UpdateInHash<DummyObject>(key, cachedObj.Id.ToString(), cachedObj);

            var updatedObj = cache.GetFromHash<DummyObject>(key, obj.Id.ToString());

            Assert.True(updatedObj.Name == "update");

            var newcount = cache.HashCount(key);

            Assert.Equal(count, newcount);
            cache.Clear();
        }


        [Fact]
        public async Task GetAllFromHash()
        {
            var key = "allhash";

            var cache = new SystemCache();

            cache.UpdateInHash<DummyObject>(key, "item1", new DummyObject() { Id = 1, Name = "test1" });

            cache.UpdateInHash<DummyObject>(key, "item2", new DummyObject() { Id = 2, Name = "test2" });

            var list = await cache.GetAllFromHash<DummyObject>(key);

            Assert.True(list.Count == 2);
        }

    }
}
