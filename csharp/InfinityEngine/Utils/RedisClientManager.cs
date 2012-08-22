using System;
using ServiceStack.Redis;

namespace InfinityEngine.Utils
{
    public sealed class RedisClientManager
    {
        //TODO: Make the host configurable
        private static BasicRedisClientManager pooledClientManager = new BasicRedisClientManager("localhost:6379");

        private RedisClientManager() {}

        public static BasicRedisClientManager get()
        {
            return pooledClientManager;
        }

    }
}
