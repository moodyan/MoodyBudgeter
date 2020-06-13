using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.User;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User
{
    public class UserProfilePropertyRepository : Repository<UserProfileProperty>
    {
        private readonly UnitOfWork Uow;

        public UserProfilePropertyRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<UserProfileProperty> Find(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.UserProfile
                                  where r.ProfileId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new ArgumentException("There is no UserProfileProperty with ID " + id + ".");
            }

            return Translate(dbRecord);
        }

        public override IQueryable<UserProfileProperty> GetAll()
        {
            return from r in Uow.DbContext.UserProfile
                   select new UserProfileProperty
                   {
                       UserProfilePropertyId = r.ProfileId,
                       UserId = r.UserId,
                       ProfilePropertyId = r.PropertyDefinitionId,
                       Value = r.PropertyValue,
                       DateUpdated = r.LastUpdatedDate,
                   };
        }

        public IQueryable<UserProfileProperty> GetAllWithRelated(bool isAdmin)
        {
            var query = from r in Uow.DbContext.UserProfile
                        select r;

            if (!isAdmin)
            {
                query = query.Where(c => c.PropertyDefinition.DefaultVisibility != (int)ProfilePropertyVisibility.Admin);
            }

            return query.Select(r => new UserProfileProperty
            {
                UserProfilePropertyId = r.ProfileId,
                UserId = r.UserId,
                ProfilePropertyId = r.PropertyDefinitionId,
                Value = r.PropertyValue,
                DateUpdated = r.LastUpdatedDate,

                ProfilePropertyName = r.PropertyDefinition.PropertyName,
                ProfilePropertyLabel = !string.IsNullOrEmpty(r.PropertyDefinition.Label) ? r.PropertyDefinition.Label : r.PropertyDefinition.PropertyName,
                Ordinal = r.PropertyDefinition.ViewOrder
            });
        }

        public IQueryable<UserProfileProperty> GetAllWithEmpty(int userId, bool isAdmin)
        {
            // The DefaultIfEmpty() makes UserProfile a left join.
            // The casting to nullable types is because the left join will return a null type if there is no corresponding record which won't fill a non nullable prop.
            var results = from ppd in Uow.DbContext.ProfilePropertyDefinition
                          from up in Uow.DbContext.UserProfile.Where(c =>
                              c.PropertyDefinitionId == ppd.PropertyDefinitionId && c.UserId == userId).DefaultIfEmpty()
                          select new { ppd, up };

            if (!isAdmin)
            {
                results = results.Where(c => c.ppd.DefaultVisibility != (int)ProfilePropertyVisibility.Admin);
            }

            return results.Select(r => new UserProfileProperty
            {
                //returns as 0 if they don't yet have this UserProfileProperty set 
                UserProfilePropertyId = (int?)r.up.ProfileId ?? 0,
                UserId = userId,
                ProfilePropertyId = r.ppd.PropertyDefinitionId,
                Value = r.up.PropertyValue,
                DateUpdated = (DateTime?)r.up.LastUpdatedDate ?? r.ppd.CreatedOnDate ?? (DateTime)SqlDateTime.MinValue, //TODO don't use min value here

                ProfilePropertyName = r.ppd.PropertyName,
                ProfilePropertyLabel = !string.IsNullOrEmpty(r.ppd.Label) ? r.ppd.Label : r.ppd.PropertyName,
                Ordinal = r.ppd.ViewOrder
            });
        }

        public IQueryable<UserSearchResponse> SearchByPropertyValue(string searchText, int propertyId, SearchOperator searchOperator, bool includePhoto)
        {
            var query = GetAll().Where(p => p.ProfilePropertyId == propertyId);

            switch (searchOperator)
            {
                case SearchOperator.Equals:
                    query = query.Where(u => string.Equals(u.Value, searchText));
                    break;
                case SearchOperator.Contains:
                    query = query.Where(u => u.Value.Contains(searchText));
                    break;
                case SearchOperator.StartsWith:
                    query = query.Where(u => u.Value.StartsWith(searchText));
                    break;
                default:
                    throw new NotSupportedException();
            }

            var displayNameDefinitionId = Uow.DbContext.ProfilePropertyDefinition.FirstOrDefault(p => p.PropertyName == "DisplayName")?.PropertyDefinitionId;

            IQueryable<UserSearchResponse> searchQueryable;

            if (!includePhoto)
            {
                searchQueryable = from match in query
                                  from displayNameUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == displayNameDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  select new UserSearchResponse
                                  {
                                      UserId = match.UserId,
                                      DisplayName = displayNameUserProfile.PropertyValue,
                                      SearchFieldValue = match.Value
                                  };
            }
            else
            {
                var avatarDefinitionId = Uow.DbContext.ProfilePropertyDefinition.FirstOrDefault(p => p.PropertyName == "Photo")?.PropertyDefinitionId;
                searchQueryable = from match in query
                                  from displayNameUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == displayNameDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  from avatarUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == avatarDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  select new UserSearchResponse
                                  {
                                      UserId = match.UserId,
                                      Avatar = avatarUserProfile.PropertyValue,
                                      DisplayName = displayNameUserProfile.PropertyValue,
                                      SearchFieldValue = match.Value
                                  };
            }

            return searchQueryable;
        }

        public IQueryable<UserSearchResponse> AddSearchResponseInfo(IQueryable<UserSearchResponse> searchQuery, bool includePhoto)
        {
            var displayNameDefinitionId = Uow.DbContext.ProfilePropertyDefinition.FirstOrDefault(p => p.PropertyName == "DisplayName")?.PropertyDefinitionId;

            var searchQueryable = from match in searchQuery
                                  from displayNameUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == displayNameDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  select new UserSearchResponse
                                  {
                                      UserId = match.UserId,
                                      DisplayName = displayNameUserProfile.PropertyValue,
                                      SearchFieldValue = match.SearchFieldValue
                                  };

            if (includePhoto)
            {
                var avatarDefinitionId = Uow.DbContext.ProfilePropertyDefinition.FirstOrDefault(p => p.PropertyName == "Photo")?.PropertyDefinitionId;
                searchQueryable = from match in searchQueryable
                                  from avatarUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == avatarDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  select new UserSearchResponse
                                  {
                                      UserId = match.UserId,
                                      Avatar = avatarUserProfile.PropertyValue,
                                      DisplayName = match.DisplayName,
                                      SearchFieldValue = match.SearchFieldValue
                                  };
            }

            return searchQueryable;
        }

        public override Task<UserProfileProperty> Create(UserProfileProperty entity)
        {
            throw new NotSupportedException();
        }

        public async Task<UserProfileProperty> CreateWithData(UserProfileProperty entity, ProfilePropertyVisibility visibility)
        {
            var dbRecord = new UserProfile
            {
                UserId = entity.UserId,
                PropertyDefinitionId = entity.ProfilePropertyId,
                PropertyValue = entity.Value,
                LastUpdatedDate = DateTime.UtcNow,
                Visibility = (int)visibility
            };

            Uow.DbContext.UserProfile.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<UserProfileProperty> Update(UserProfileProperty entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.UserProfile
                                  where r.ProfileId == entity.UserProfilePropertyId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no UserProfileProperty with ID " + entity.UserProfilePropertyId + ".");
            }

            dbRecord.PropertyValue = entity.Value;
            dbRecord.LastUpdatedDate = DateTime.UtcNow;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.UserProfile
                                  where r.ProfileId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("UserProfileProperty does not exist.");
            }

            Uow.DbContext.UserProfile.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private UserProfileProperty Translate(UserProfile dbRecord)
        {
            return new UserProfileProperty
            {
                UserProfilePropertyId = dbRecord.ProfileId,
                UserId = dbRecord.UserId,
                ProfilePropertyId = dbRecord.PropertyDefinitionId,
                Value = dbRecord.PropertyValue,
                DateUpdated = dbRecord.LastUpdatedDate,
            };
        }
    }
}
