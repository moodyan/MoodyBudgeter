using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Settings
{
    public class SiteListEntryCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "Setting:ListEntries:";
        
        private readonly IBudgeterCache Cache;

        public SiteListEntryCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<List<SiteListEntry>> GetSiteEntriesFromCache(int listId, bool isAdmin)
        {
            var key = GetCacheKey(listId, isAdmin);

            return await Cache.Get<List<SiteListEntry>>(key);
        }

        internal async Task AddListEntryToCache(int listId, List<SiteListEntry> siteLists, bool isAdmin)
        {
            var key = GetCacheKey(listId, isAdmin);

            await Cache.Insert(key, siteLists, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateListEntryCache(int listId)
        {
            await Cache.Remove(GetCacheKey(listId, false));
            await Cache.Remove(GetCacheKey(listId, true));
        }

        private string GetCacheKey(int listId, bool isAdmin)
        {
            return CACHE_KEY_PREFIX + "_ListId-" + listId + "_IsAdmin-" + isAdmin;
        }
    }
}
