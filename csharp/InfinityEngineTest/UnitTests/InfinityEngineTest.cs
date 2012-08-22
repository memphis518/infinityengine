using System;
using System.Text;
using System.Collections.Generic;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using InfinityEngine.Models;
using InfinityEngine.Utils;
using InfinityEngine.Business;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace InfinityEngineTest.UnitTests
{    
  
    [TestClass]
    public class InfinityEngineTest
    {

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                redisClient.FlushAll();
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                redisClient.FlushAll();
            }
        }


        [TestMethod]
        public void TestSaveNewSearchRoute()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "code";
            config.UpdateURL = "http://urltoupdatefrom";

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            infinityEngine.SaveSearchRoute(config);

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                Configuration redisConfig = redisClient.Get<Configuration>(config.AutoCompleteRoute);
                Assert.IsTrue(redisConfig.AutoCompleteRoute.Equals(config.AutoCompleteRoute));                               
            }


        }

        [TestMethod]
        public void TestUpdateSearchRoute()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {              
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);
            }    
                InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
                config.RecordIndentifier = "key";
                infinityEngine.SaveSearchRoute(config);

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                Configuration redisConfig = redisClient.Get<Configuration>(config.AutoCompleteRoute);
                Assert.IsTrue(redisConfig.RecordIndentifier.Equals("key"));
                
            }
        }

        [TestMethod]
        public void TestRemoveSearchRoute()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {

                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);
            }
                
            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            infinityEngine.RemoveSearchRoute(config.AutoCompleteRoute);

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                Configuration redisConfig = redisClient.Get<Configuration>(config.AutoCompleteRoute);
                Assert.IsNull(redisConfig);                
            }
        }


        [TestMethod]
        public void TestUpdateFullCacheAll()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "id";
            config.UpdateURL = "http://localhost:8124/names";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);
            }
            
            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            infinityEngine.UpdateFullCache(config.AutoCompleteRoute);

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                string data = redisClient.GetValue("fullCache:" + config.AutoCompleteRoute + ":123");                            
                Assert.IsNotNull(data);
            }
        }

        [TestMethod]
        public void TestUpdateFullCachePartial()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "id";
            config.UpdateURL = "http://localhost:8124/names";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);
            }

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            infinityEngine.UpdateFullCache(config.AutoCompleteRoute);
            infinityEngine.UpdateFullCache(config.AutoCompleteRoute, "stephen");

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                string data = redisClient.GetValue("fullCache:" + config.AutoCompleteRoute + ":567");
                Dictionary<string, string> dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);                
                Assert.IsTrue(dataDic["lastname"].Equals("smith"));
            }
        }


        [TestMethod]
        public void TestUpdateAutoCompleteCache()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "Code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);

                Dictionary<string,string> dataObj = new Dictionary<string,string>();
                dataObj["Code"] = "foo";
                dataObj["Desc"] = "bar";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj)); 
            }

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            infinityEngine.UpdateAutoCompleteCache(config.AutoCompleteRoute);

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                string setName = config.AutoCompleteRoute + ":autocomplete:ba";
                List<string> setValues = redisClient.SortedSets[setName].GetAll();

                Assert.IsTrue(setValues.Count > 0);
            }

        }

        [TestMethod]
        public void TestSearchAutoCompleteCache()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "Code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);

                Dictionary<string,string> dataObj = new Dictionary<string,string>();
                dataObj["Code"] = "foo";
                dataObj["Desc"] = "bar";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar*", "foo", 0);
            }

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();            
            List<Dictionary<string, string>> results = infinityEngine.SearchAutoCompleteCache(config.AutoCompleteRoute, "ba", 50);

            if (results.Count > 0)
            {
                Dictionary<string, string> firstResult = results[0];
                Assert.IsTrue(firstResult["Code"].Equals("foo") &&
                              firstResult["Desc"].Equals("bar"));
            }
            else
            {
                Assert.IsTrue(false);
            }

        }

        [TestMethod]
        public void TestSearchAutoCompleteCacheWithMaxResults()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 1;
            config.RecordIndentifier = "Code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);

                Dictionary<string, string> dataObj = new Dictionary<string, string>();
                dataObj["Code"] = "foo";
                dataObj["Desc"] = "bar";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar*", "foo", 0);

                Dictionary<string, string> dataObj2 = new Dictionary<string, string>();
                dataObj2["Code"] = "yo";
                dataObj2["Desc"] = "barber";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj2[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj2[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj2));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barb", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barbe", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barber", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barber*", "yo", 0);
            }

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            List<Dictionary<string, string>> results = infinityEngine.SearchAutoCompleteCache(config.AutoCompleteRoute, "ba", 50);
            
            Assert.IsTrue(results.Count == 1);
        }

        [TestMethod]
        public void TestSearchAutoCompleteCacheWithHighMaxResults()
        {
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 4000;
            config.RecordIndentifier = "Code";
            config.UpdateURL = "http://urltoupdatefrom";

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                redis.SetEntry(config.AutoCompleteRoute, config);

                Dictionary<string, string> dataObj = new Dictionary<string, string>();
                dataObj["Code"] = "foo";
                dataObj["Desc"] = "bar";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar*", "foo", 0);

                Dictionary<string, string> dataObj2 = new Dictionary<string, string>();
                dataObj2["Code"] = "yo";
                dataObj2["Desc"] = "barber";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj2[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj2[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj2));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barb", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barbe", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barber", "yo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:barber*", "yo", 0);
            }

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            List<Dictionary<string, string>> results = infinityEngine.SearchAutoCompleteCache(config.AutoCompleteRoute, "ba", 50);

            Assert.IsTrue(results.Count == 2);
        }
    }
}
