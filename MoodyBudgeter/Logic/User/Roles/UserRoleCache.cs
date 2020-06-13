using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Roles
{
    public class UserRoleCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "User:Roles:";
        
        private readonly IBudgeterCache Cache;

        public UserRoleCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<List<UserRole>> GetUserRolesFromCache(int userId, bool isAdmin)
        {
            var key = GetCacheKey(userId, isAdmin);

            return await Cache.Get<List<UserRole>>(key);
        }

        internal async Task AddUserRolesToCache(int userId, List<UserRole> roles, bool isAdmin)
        {
            var key = GetCacheKey(userId, isAdmin);

            await Cache.Insert(key, roles, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateUserRoleCache(int userId, bool isAdmin)
        {
            string key = GetCacheKey(userId, isAdmin);

            await Cache.Remove(key);
        }

        public async Task InvalidateUserRoleCache(int userId)
        {
            await Cache.Remove(GetCacheKey(userId, true));
            await Cache.Remove(GetCacheKey(userId, false));
        }

        public async Task InvalidateUserRoleCache()
        {
            await Cache.ScanRedisAndRemovePrefix(CACHE_KEY_PREFIX);
        }

        private string GetCacheKey(int userId, bool isAdmin)
        {
            return CACHE_KEY_PREFIX + "-UserID_" + userId + "-isAdmin_" + isAdmin;
        }
    }
}
