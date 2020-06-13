using MoodyBudgeter.Models.User;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UserCacheLogic
    {
        private const int CACHE_TIME_IN_HOURS = 6;
        private const string CACHE_KEY_PREFIX = "User:";
        
        private readonly IBudgeterCache Cache;
        
        public UserCacheLogic(IBudgeterCache cache)
        {
            Cache = cache;
        }

        public async Task<BudgetUser> GetUserFromCache(int userId)
        {
            var key = GetCacheKey(userId);

            return await Cache.Get<BudgetUser>(key);
        }

        public async Task AddUserToCache(BudgetUser user)
        {
            var key = GetCacheKey(user.UserId);

            await Cache.Insert(key, user, new TimeSpan(CACHE_TIME_IN_HOURS, 0, 0));
        }

        public async Task InvalidateUserCache(int userId)
        {
            var key = GetCacheKey(userId);

            await Cache.Remove(key);
        }

        private string GetCacheKey(int userId)
        {
            return CACHE_KEY_PREFIX + "-UserID_" + userId;
        }
    }
}
