using System;
using InfinityEngine;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InfinityEngine.Services;
using InfinityEngine.Models;
using InfinityEngine.Utils;
using Newtonsoft.Json;


namespace InfinityEngineTest.ServiceTests
{
    [TestClass]
    public class InfinityEngineServiceTest
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
        public void ConfigureNewSearchRouteTestNewConfig()
        {
            InfinityEngine.Services.InfinityEngineService infinityEngineService = new InfinityEngine.Services.InfinityEngineService();
            Configuration config = new Configuration();
            config.AutoCompleteRoute = "testRoute";
            config.MaxResults = 100;
            config.RecordIndentifier = "id";
            config.UpdateURL = "http://localhost:8124/names";

            infinityEngineService.ConfigureSearchRoute(config);

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();
                Configuration redisConfig = redisClient.Get<Configuration>(config.AutoCompleteRoute);
                Assert.IsTrue(redisConfig.AutoCompleteRoute.Equals(config.AutoCompleteRoute));                
            }
        }

        [TestMethod]
        public void UpdateSearchRouteAllTest()
        {
            InfinityEngine.Services.InfinityEngineService infinityEngineService = new InfinityEngine.Services.InfinityEngineService();
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

            infinityEngineService.UpdateSearchRouteAll(config.AutoCompleteRoute);

            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                string data = redisClient.Get<string>("fullCache:" + config.AutoCompleteRoute + ":123");
                Assert.IsNotNull(data);
            }

        }

        [TestMethod]
        public void SearchTheRouteTest()
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

                Dictionary<string, string> dataObj = new Dictionary<string, string>();
                dataObj["Code"] = "foo";
                dataObj["Desc"] = "bar";
                redisClient.Lists["cacheIndex:" + config.AutoCompleteRoute].Add(dataObj[config.RecordIndentifier]);
                redisClient.SetEntry("fullCache:" + config.AutoCompleteRoute + ":" + dataObj[config.RecordIndentifier], JsonConvert.SerializeObject(dataObj));

                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:b", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:ba", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar", "foo", 0);
                redisClient.AddItemToSortedSet(config.AutoCompleteRoute + ":autocomplete:bar*", "foo", 0);
            }

            InfinityEngine.Services.InfinityEngineService infinityEngineService = new InfinityEngine.Services.InfinityEngineService();
            string resultString = infinityEngineService.SearchTheRoute(config.AutoCompleteRoute, "ba");
            List<Dictionary<string, string>> results = JsonConvert.DeserializeObject <List<Dictionary<string, string>>>(resultString);

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
        public void SearchTheRouteTestLimit()
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

            InfinityEngine.Services.InfinityEngineService infinityEngineService = new InfinityEngine.Services.InfinityEngineService();
            string resultString = infinityEngineService.SearchTheRouteWithLimit(config.AutoCompleteRoute, "ba", "1");
            List<Dictionary<string, string>> results = JsonConvert.DeserializeObject <List<Dictionary<string, string>>>(resultString);

            Assert.IsTrue(results.Count == 1);
            
        }


    }
}
