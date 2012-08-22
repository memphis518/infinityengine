using System;
using System.Collections.Generic;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using InfinityEngine.Business;
using InfinityEngine.Interfaces;
using InfinityEngine.Models;
using Newtonsoft.Json;

namespace InfinityEngine.Services
{
    public class InfinityEngineService : IInfinityEngineService
    {
        public bool ConfigureSearchRoute(Configuration newConfig)
        {
            bool success = false;

            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            success = infinityEngine.SaveSearchRoute(newConfig);
            
            return success;
        }

        public bool UpdateSearchRouteAll(string routeName)
        {
            return UpdateSearchRoute(routeName, null);
        }

        public bool UpdateSearchRoute(string routeName, string updateKey)
        {
            bool success = false;
            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            success = infinityEngine.UpdateFullCache(routeName, updateKey);
            if (success)
            {
                success = false;
                infinityEngine.UpdateAutoCompleteCache(routeName);
                success = true;
            }
            return success;            
        }

        public string SearchTheRoute(string routeName, string searchParam)
        {
            return SearchTheRouteWithLimit(routeName, searchParam, "1000");
        }

        public string SearchTheRouteWithLimit(string routeName, string searchParam, string resultLimit)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            
            InfinityEngine.Business.InfinityEngine infinityEngine = new InfinityEngine.Business.InfinityEngine();
            results = infinityEngine.SearchAutoCompleteCache(routeName, searchParam, int.Parse(resultLimit));
            return JsonConvert.SerializeObject(results);
        }
        
    }
}
