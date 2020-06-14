using MoodyBudgeter.Utility.Cache.Redis;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Cache
{
    /// <summary>
    /// This class holds the logic to cache our data in either Redis or HTTPRuntime if Redis is not configured.
    /// If Redis goes down, this class will punt exceptions. Does it make sense to catch exceptions here and return null?
    /// </summary>
    public sealed class BudgeterCache : IBudgeterCache
    {
        private const string KEY_PREFIX = "MoodyBudgeter:";
        private readonly RedisStringRepository Repo;

        public BudgeterCache()
        {
            Repo = new RedisStringRepository();
        }

        /// <summary>
        /// Retrieves the specified item from the Cache using the generic type.
        /// </summary>
        /// <param name="key">The identifier for the cache item to retrieve.</param>
        /// <returns>The retrieved cache item, or null if the key is not found.</returns>
        public async Task<T> Get<T>(string key)
        {
            T results = await Repo.GetById<T>(KEY_PREFIX + key);

            return results;
        }

        /// <summary>
        /// Adds an object into the Cache object with expiration.
        /// </summary>
        /// <param name="key">The cache key that is used to reference the object.</param>
        /// <param name="value">The object to insert into the cache.</param>
        /// <param name="TimeToLive">How long the value will persist in cache for.</param>
        public async Task Insert(string key, object value, TimeSpan timeToLive)
        {
            await Repo.Create(KEY_PREFIX + key, value, timeToLive);
        }

        /// <summary>
        /// Removes the specified item from the application's Cache.
        /// </summary>
        /// <param name="key">The key identifier for the cache item to remove.</param>
        public async Task Remove(string key)
        {
            await Repo.Delete(KEY_PREFIX + key);
        }

        /// <summary>
        /// Removes a collection of keys from cache that match a prefix.
        /// WARNING: Use very specific prefixes to not delete extra keys and keep the processing time down.
        /// </summary>
        /// <param name="Prefix">The key prefix that a key has to start with to be deleted.</param>
        public async Task ScanRedisAndRemovePrefix(string prefix)
        {
            await Repo.RemoveWithPrefix(KEY_PREFIX + prefix);
        }
    }
}
