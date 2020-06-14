using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Cache.Redis
{
    /// <summary>
    /// Repository for interacting with simple string redis keys.
    /// </summary>
    public class RedisStringRepository
    {
        private readonly IDatabase DB;
        private const int SCAN_PAGE_SIZE = 2000;

        public RedisStringRepository()
        {
            DB = RedisConnection.Connection.GetDatabase();
        }

        public async Task<T> GetById<T>(string id)
        {
            string value = await DB.StringGetAsync(id);

            return Deserialize<T>(value);
        }

        public async Task Create(string id, object entity, TimeSpan timeToLive)
        {
            string SerializedData = Serialize(entity);

            await DB.StringSetAsync(id, SerializedData, timeToLive);
        }

        public async Task Delete(string id)
        {
            await DB.KeyDeleteAsync(id);
        }

        private string Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj);
        }

        private T Deserialize<T>(string value)
        {
            if (value == null)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        public IEnumerable<RedisKey> GetAllRedisKeys(IEnvironmentRequester environmentRequester, string prefix)
        {
            // If we move to multiple Redis servers, this will not work.
            var connections = RedisConnection.Connection.GetEndPoints();
            var endpoint = connections.FirstOrDefault();

            var server = RedisConnection.Connection.GetServer(endpoint);

            int dbNumber = int.Parse(environmentRequester.GetVariable("RedisDB"));

            return server.Keys(dbNumber, prefix + "*", SCAN_PAGE_SIZE);
        }

        public async Task RemoveWithPrefix(string keyPrefix)
        {
            foreach (var key in GetAllRedisKeys(new EnvironmentRequester(), keyPrefix))
            {
                await Delete(key);
            }
        }
    }
}
