using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Roles
{
    public class RoleCache
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "Roles:";
        
        private readonly IBudgeterCache Cache;

        public RoleCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<List<Role>> GetRolesFromCache(bool isAdmin)
        {
            var Key = GetCacheKey(isAdmin);

            return await Cache.Get<List<Role>>(Key);
        }

        internal async Task AddRolesToCache(List<Role> roles, bool isAdmin)
        {
            var Key = GetCacheKey(isAdmin);

            await Cache.Insert(Key, roles, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateRolesCache()
        {
            await Cache.ScanRedisAndRemovePrefix(CACHE_KEY_PREFIX);
        }

        private string GetCacheKey(bool isAdmin)
        {
            return CACHE_KEY_PREFIX + "-isAdmin_" + isAdmin;
        }
    }
}
