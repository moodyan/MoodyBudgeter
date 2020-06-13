using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Profile
{
    public class ProfilePropertyCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "User:ProfileProperty:";
        
        private readonly IBudgeterCache Cache;
        
        public ProfilePropertyCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        public async Task<List<ProfileProperty>> GetProfilePropertiesFromCache(bool requiredOnly, bool isAdmin)
        {
            var key = GetCacheKey(requiredOnly, isAdmin);

            return await Cache.Get<List<ProfileProperty>>(key);
        }

        public async Task AddProfilePropertiesToCache(List<ProfileProperty> profileProperties, bool requiredOnly, bool isAdmin)
        {
            var key = GetCacheKey(requiredOnly, isAdmin);

            await Cache.Insert(key, profileProperties, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateProfilePropertiesCache()
        {
            await Cache.ScanRedisAndRemovePrefix(CACHE_KEY_PREFIX);
        }

        private string GetCacheKey(bool requiredOnly, bool isAdmin)
        {
            return CACHE_KEY_PREFIX + "_RequiredOnly-" + requiredOnly + "_IsAdmin-" + isAdmin;
        }
    }
}
