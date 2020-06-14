using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Repositories.User.Profile;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Profile
{
    public class ProfilePropertyLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public ProfilePropertyLogic(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<List<ProfileProperty>> GetProfileProperties(bool requiredOnly, bool isAdmin)
        {
            ProfilePropertyCache profilePropertyCache = new ProfilePropertyCache(Cache);

            List<ProfileProperty> profileProperties = await profilePropertyCache.GetProfilePropertiesFromCache(requiredOnly, isAdmin);

            if (profileProperties != null)
            {
                return profileProperties;
            }

            using (UnitOfWork uow = new UnitOfWork(Context))
            {
                ProfilePropertyRepository repo = new ProfilePropertyRepository(uow);

                IQueryable<ProfileProperty> query = repo.GetAll();

                if (!isAdmin)
                {
                    query = query.Where(c => c.Visibility != ProfilePropertyVisibility.Admin);
                }

                if (requiredOnly)
                {
                    query = query.Where(c => c.Required);
                }

                profileProperties = await query.ToListAsync();
            }

            await profilePropertyCache.AddProfilePropertiesToCache(profileProperties, requiredOnly, isAdmin);

            return profileProperties;
        }
        
        public async Task<ProfileProperty> GetProfileProperty(int profilePropertyId, bool isAdmin)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new ProfilePropertyRepository(uow);

                var query = repo.GetAll().Where(x => x.ProfilePropertyId == profilePropertyId);

                if (!isAdmin)
                {
                    query = query.Where(c => c.Visibility != ProfilePropertyVisibility.Admin);
                }

                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<ProfileProperty> GetProfileProperty(string name, bool isAdmin)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new ProfilePropertyRepository(uow);

                var query = repo.GetAll().Where(c => c.Name.ToUpper() == name.ToUpper());

                if (!isAdmin)
                {
                    query = query.Where(c => c.Visibility != ProfilePropertyVisibility.Admin);
                }

                return await query.FirstOrDefaultAsync();
            }
        }

        public async Task<ProfileProperty> AddProfileProperty(ProfileProperty profileProperty)
        {
            ValidateProfileProperty(profileProperty);

            var existingProperty = await GetProfileProperty(profileProperty.Name, true);

            if (existingProperty != null)
            {
                throw new CallerException("A ProfileProperty already exists with that name");
            }

            ProfileProperty createdProfileProperty;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new ProfilePropertyRepository(uow);

                createdProfileProperty = await repo.Create(profileProperty);
            }

            await new ProfilePropertyCache(Cache).InvalidateProfilePropertiesCache();
            await new UserProfilePropertyCache(Cache).InvalidateUserProfilePropertiesCache();

            return createdProfileProperty;
        }

        private void ValidateProfileProperty(ProfileProperty profileProperty)
        {
            if (string.IsNullOrEmpty(profileProperty.PropertyCategory))
            {
                throw new CallerException("You must provide a category to create a profile property");
            }

            if (string.IsNullOrEmpty(profileProperty.Name))
            {
                throw new CallerException("You must provide a property name to create a profile property");
            }
        }

        public async Task<ProfileProperty> UpdateProfileProperty(ProfileProperty profileProperty)
        {
            ProfileProperty updatedProfileProperty;

            var existingProperty = await GetProfileProperty(profileProperty.ProfilePropertyId, true);

            if (existingProperty.Name != profileProperty.Name)
            {
                var sameNamedProperty = await GetProfileProperty(profileProperty.Name, true);

                if (sameNamedProperty != null)
                {
                    throw new CallerException("A ProfileProperty already exists with that name");
                }
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new ProfilePropertyRepository(uow);

                updatedProfileProperty = await repo.Update(profileProperty);
            }

            await new ProfilePropertyCache(Cache).InvalidateProfilePropertiesCache();
            await new UserProfilePropertyCache(Cache).InvalidateUserProfilePropertiesCache();

            return updatedProfileProperty;
        }

        public async Task<Page<ProfileProperty>> GetGrid(GridRequest gridRequest)
        {
            var data = new Page<ProfileProperty>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new ProfilePropertyRepository(uow);

                var query = repo.GetAll();

                var dataGridLogic = new DataGridLogic<ProfileProperty>(gridRequest, query);

                data.Records = await dataGridLogic.GetResults();
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }
    }
}
