using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Settings
{
    public class SiteListCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "Setting:SiteLists:";
        
        private readonly IBudgeterCache Cache;

        public SiteListCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<List<SiteList>> GetSiteListsFromCache(bool isAdmin)
        {
            var key = GetCacheKey(isAdmin);

            return await Cache.Get<List<SiteList>>(key);
        }

        internal async Task AddSiteListToCache(List<SiteList> siteLists, bool isAdmin)
        {
            var key = GetCacheKey(isAdmin);

            await Cache.Insert(key, siteLists, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateSiteListCache()
        {
            await Cache.Remove(GetCacheKey(false));
            await Cache.Remove(GetCacheKey(true));
        }

        private string GetCacheKey(bool isAdmin)
        {
            return CACHE_KEY_PREFIX + "_IsAdmin-" + isAdmin;
        }
    }
}
