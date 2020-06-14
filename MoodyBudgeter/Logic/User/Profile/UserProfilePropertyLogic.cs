using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UserProfilePropertyLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public UserProfilePropertyLogic(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        // Gets a users profile. This will return empty values for Properties the user does not fill out.
        public async Task<List<UserProfileProperty>> GetUserProfileProperties(int userId, bool isAdmin)
        {
            var cache = new UserProfilePropertyCache(Cache);

            List<UserProfileProperty> userProfileProperties = await cache.GetUserProfilePropertiesFromCache(userId, isAdmin);

            if (userProfileProperties != null)
            {
                return userProfileProperties;
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                var query = repo.GetAllWithEmpty(userId, isAdmin);

                userProfileProperties = await query.ToListAsync();
            }

            await cache.AddUserProfilePropertiesToCache(userId, isAdmin, userProfileProperties);

            return userProfileProperties;
        }

        public async Task<UserProfileProperty> GetUserProfileProperty(int userId, int profilePropertyId, bool isAdmin)
        {
            return await GetUserProfileProperty(userId, profilePropertyId, "", isAdmin);
        }

        public async Task<UserProfileProperty> GetUserProfileProperty(int userId, string name, bool isAdmin)
        {
            return await GetUserProfileProperty(userId, 0, name, isAdmin);
        }

        private async Task<UserProfileProperty> GetUserProfileProperty(int userId, int profilePropertyId, string name, bool isAdmin)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                IQueryable<UserProfileProperty> query;

                if (profilePropertyId == 0)
                {
                    query = repo.GetAllWithEmpty(userId, isAdmin).Where(c => c.ProfilePropertyName.ToUpper() == name.ToUpper());
                }
                else
                {
                    query = repo.GetAllWithEmpty(userId, isAdmin).Where(c => c.ProfilePropertyId == profilePropertyId);
                }

                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<UserProfileProperty> GetUserProfileProperty(int userProfilePropertyId, bool isAdmin)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                var query = repo.GetAllWithRelated(isAdmin).Where(c => c.UserProfilePropertyId == userProfilePropertyId);

                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<List<int>> FindUsersFromValue(int profilePropertyId, string value)
        {
            return await FindUsersFromValue(profilePropertyId, "", value);
        }

        public async Task<List<int>> FindUsersFromValue(string profilePropertyName, string value)
        {
            return await FindUsersFromValue(0, profilePropertyName, value);
        }

        private async Task<List<int>> FindUsersFromValue(int profilePropertyId, string profilePropertyName, string value)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                IQueryable<UserProfileProperty> query;

                if (profilePropertyId == 0)
                {
                    query = repo.GetAllWithRelated(true).Where(c => c.ProfilePropertyName.ToUpper() == profilePropertyName.ToUpper());
                }
                else
                {
                    query = repo.GetAllWithRelated(true).Where(c => c.ProfilePropertyId == profilePropertyId);
                }

                query = query.Where(c => c.Value.ToUpper() == value.ToUpper());

                return await query.Select(c => c.UserId).ToListAsync();
            }
        }

        public string GetValueFromList(List<UserProfileProperty> userProfileProperties, string name)
        {
            return (from c in userProfileProperties
                    where c.ProfilePropertyName.ToUpper() == name.ToUpper()
                    select c.Value).FirstOrDefault();
        }

        public string GetValueFromList(List<UserProfileProperty> userProfileProperties, int profilePropertyId)
        {
            return (from c in userProfileProperties
                    where c.ProfilePropertyId == profilePropertyId
                    select c.Value).FirstOrDefault();
        }
    }
}
