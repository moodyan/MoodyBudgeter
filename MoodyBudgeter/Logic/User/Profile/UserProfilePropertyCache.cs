using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Profile
{
    public class UserProfilePropertyCache
    {
        private const int CACHE_TIME_IN_HOURS = 2;
        private const string CACHE_KEY_PREFIX = "User:UserProfile:";
        
        private readonly IBudgeterCache Cache;
        
        public UserProfilePropertyCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        public async Task<List<UserProfileProperty>> GetUserProfilePropertiesFromCache(int userId, bool showAdminProperties)
        {
            var key = GetCacheKey(userId, showAdminProperties);

            return await Cache.Get<List<UserProfileProperty>>(key);
        }

        public async Task AddUserProfilePropertiesToCache(int userId, bool showAdminProperties, List<UserProfileProperty> userProfileProperties)
        {
            var key = GetCacheKey(userId, showAdminProperties);

            await Cache.Insert(key, userProfileProperties, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateUserProfilePropertiesCache(int userId)
        {
            await Cache.Remove(GetCacheKey(userId, true));
            await Cache.Remove(GetCacheKey(userId, false));
        }

        public async Task InvalidateUserProfilePropertiesCache()
        {
            await Cache.ScanRedisAndRemovePrefix(CACHE_KEY_PREFIX);
        }

        private string GetCacheKey(int userId, bool showAdminProperties)
        {
            return CACHE_KEY_PREFIX + "_UserId-" + userId + "_ShowAdminProperties-" + showAdminProperties;
        }
    }
}