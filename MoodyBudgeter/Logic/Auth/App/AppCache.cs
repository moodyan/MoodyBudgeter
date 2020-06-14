using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Models.Auth.App;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.App
{
    public class AppCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "Auth:App:";
        
        private readonly IBudgeterCache Cache;

        public AppCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<AppModel> GetAppFromCache(string clientId)
        {
            var Key = GetCacheKey(clientId);

            return await Cache.Get<AppModel>(Key);
        }

        internal async Task AddAppToCache(string clientId, AppModel app)
        {
            var Key = GetCacheKey(clientId);

            await Cache.Insert(Key, app, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateAppCache(string clientId)
        {
            var Key = GetCacheKey(clientId);

            await Cache.Remove(Key);
        }

        private string GetCacheKey(string clientId)
        {
            return CACHE_KEY_PREFIX + "-ClientId_" + clientId;
        }
    }
}
