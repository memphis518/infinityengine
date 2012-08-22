using System;
using System.Text;
using System.Collections.Generic;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using InfinityEngine.Models;
using InfinityEngine.Utils;
using InfinityEngine.Business.WebPoller;
using Newtonsoft.Json;

namespace InfinityEngine.Business
{
    public class InfinityEngine
    {

        public bool SaveSearchRoute(Configuration config)
        {
            bool success = false;

            using (IRedisTypedClient<Configuration> redis = GetRedisClientForConfigurations())
            {
                redis.SetEntry(config.AutoCompleteRoute, config);
                success = true;
            }
                     
            return success;
        }
                
        public bool RemoveSearchRoute(string routeName)
        {

            bool success = false;

            using (IRedisTypedClient<Configuration> redis = GetRedisClientForConfigurations())
            {
                success = redis.RemoveEntry(routeName);              
            }

            return success;
        }

        public bool UpdateFullCache(string routeName, string searchString = null)
        {
            bool success = false;

            Configuration config;
            using (IRedisTypedClient<Configuration> redis = GetRedisClientForConfigurations())
            {
                config = redis.GetValue(routeName);
            }

            if (config != null)
            {
                IWebPoller webPoller = new JsonWebPoller();
                string url = "";
                if (searchString == null)
                {
                    url = config.UpdateURL;
                }else
                {
                    url = config.UpdateURL + "/" + searchString;
                }

                List<Dictionary<string, string>> results = (List<Dictionary<string, string>>) webPoller.PollURL(url);
                if (searchString == null)
                {
                    updateAllRouteObjectsCache(routeName, config.RecordIndentifier, results);
                }
                else
                {
                    updateRouteObjectsCache(routeName, config.RecordIndentifier, results);
                }
                success = true;
            }
            
            return success;
        }

        public void UpdateAutoCompleteCache(string routeName)
        {
            Configuration config;
            using (IRedisTypedClient<Configuration> redis = GetRedisClientForConfigurations())
            {
                config = redis.GetValue(routeName);
            }

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();

            List<string> cacheIndices;
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                cacheIndices = redisClient.Lists["cacheIndex:" + routeName].GetAll();
            }

            foreach (string index in cacheIndices)
            {

                Dictionary<string, string> dataDic = null;
                using (IRedisClient redisClient = pooledClientManager.GetClient())
                {
                    string data = redisClient.GetValue("fullCache:" + routeName + ":" + index);      
                    dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                }
                foreach (KeyValuePair<string, string> pair in dataDic)
                {
                    if(pair.Key.Equals(config.RecordIndentifier))
                    {
                        continue;
                    }
                    else
                    {
                        using (IRedisClient redisClient = pooledClientManager.GetClient())
                        {
                            string prefix = "";
                            foreach (char c in pair.Value)
                            {
                                prefix = prefix + c;
                                prefix = prefix.ToLower();                                
                                redisClient.AddItemToSortedSet(routeName + ":autocomplete:" + prefix, index, 0);
                            }
                            redisClient.AddItemToSortedSet(routeName + ":autocomplete:" + pair.Value + "*", index, 0);
                        }
                    }
                }               
            }
        }

        public List<Dictionary<string, string>> SearchAutoCompleteCache(string routeName, string searchParam, int resultLimit)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                string setID = routeName + ":autocomplete:" + searchParam;
                List<string> matchingIndices = redisClient.GetRangeFromSortedSet(setID, 0, resultLimit - 1);
                foreach(string index in matchingIndices)
                {
                    string data = redisClient.GetValue("fullCache:" + routeName + ":" + index);
                    Dictionary<string, string> dataDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                    results.Add(dataDic);
                }
            }
            return results;
        }

        private void removeAllHashesForCache(string routeName)
        {

            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                List<string> cacheIndex = redisClient.Lists["cacheIndex:" + routeName].GetAll();
                foreach (string index in cacheIndex)
                {
                    redisClient.RemoveEntry("fullCache:" + routeName + ":" + index);
                }
                redisClient.RemoveAllFromList("cacheIndex:" + routeName);

            }
        }

        private void updateAllRouteObjectsCache(string routeName, string recordIdentifier, List<Dictionary<string, string>> data)
        {
            BasicRedisClientManager pooledClientManager = RedisClientManager.get();

            removeAllHashesForCache(routeName);
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {
                foreach ( Dictionary<string, string> dataObj in data)
                {
                    redisClient.Lists["cacheIndex:" + routeName].Add(dataObj[recordIdentifier]);
                    redisClient.SetEntry("fullCache:" + routeName + ":" + dataObj[recordIdentifier], JsonConvert.SerializeObject(dataObj));                    
                }
            }
        }

        private void updateRouteObjectsCache(string routeName, string recordIdentifier, List<Dictionary<string, string>> data)
        {
            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            using (IRedisClient redisClient = pooledClientManager.GetClient())
            {                
                foreach ( Dictionary<string, string> dataObj in data)
                {
                    redisClient.RemoveEntry("fullCache:" + routeName + ":" + dataObj[recordIdentifier]);
                    redisClient.Lists["cacheIndex:" + routeName].Add(dataObj[recordIdentifier]);
                    redisClient.SetEntry("fullCache:" + routeName + ":" + dataObj[recordIdentifier], JsonConvert.SerializeObject(dataObj)); 
                }
            }
        }

        private IRedisTypedClient<Configuration> GetRedisClientForConfigurations()
        {
            BasicRedisClientManager pooledClientManager = RedisClientManager.get();
            IRedisClient redisClient = pooledClientManager.GetClient();

            IRedisTypedClient<Configuration> redis = redisClient.As<Configuration>();

            return redis;
        }

    }
}
