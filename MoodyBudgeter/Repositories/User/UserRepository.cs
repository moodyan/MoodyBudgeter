using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.User;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User
{
    public class UserRepository : Repository<BudgetUser>
    {
        private readonly UnitOfWork Uow;

        public UserRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<BudgetUser> Find(int id)
        {
            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.Users
                                  where r.UserId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no User with Id " + id + ".");
            }

            // Return the response data.
            return Translate(dbRecord);
        }

        public override IQueryable<BudgetUser> GetAll()
        {
            return from r in Uow.DbContext.Users
                   select new BudgetUser
                   {
                       UserId = r.UserId,
                       Username = r.Username
                   };
        }

        public IQueryable<GridUser> GetAllForGrid(int? profilePropertyId, string searchText, SearchOperator? searchOperator, bool includeDeleted)
        {
            var query = from u in Uow.DbContext.Users
                        select u;

            if (!includeDeleted)
            {
                query = query.Where(r => !r.IsDeleted);
            }

            if (profilePropertyId.HasValue)
            {
                switch (searchOperator)
                {
                    case SearchOperator.Contains:
                        query = from q in query
                                join prof in Uow.DbContext.UserProfile on q.UserId equals prof.UserId
                                where prof.PropertyDefinitionId == profilePropertyId &&
                                      prof.PropertyValue.Contains(searchText)
                                select q;

                        break;
                    case SearchOperator.Equals:
                        query = from q in query
                                join prof in Uow.DbContext.UserProfile on q.UserId equals prof.UserId
                                where prof.PropertyDefinitionId == profilePropertyId &&
                                      prof.PropertyValue.Equals(searchText)
                                select q;
                        break;
                    case SearchOperator.StartsWith:
                        query = from q in query
                                join prof in Uow.DbContext.UserProfile on q.UserId equals prof.UserId
                                where prof.PropertyDefinitionId == profilePropertyId &&
                                      prof.PropertyValue.StartsWith(searchText)
                                select q;
                        break;
                    default:
                        throw new CallerException("Must specify a search operator for Grid request");
                }
            }

            return query.Select(r => new GridUser
            {
                UserId = r.UserId,
                Username = r.Username,
                Email = r.UserProfile.FirstOrDefault(c => c.PropertyDefinition.PropertyName == "Email").PropertyValue,
                DisplayName = r.UserProfile.FirstOrDefault(c => c.PropertyDefinition.PropertyName == "DisplayName").PropertyValue,
                FirstName = r.UserProfile.FirstOrDefault(c => c.PropertyDefinition.PropertyName == "FirstName").PropertyValue,
                LastName = r.UserProfile.FirstOrDefault(c => c.PropertyDefinition.PropertyName == "LastName").PropertyValue,
            });
        }

        public IQueryable<UserSearch> SearchByUsername(string searchText, SearchOperator searchOperator, bool includePhoto)
        {
            IQueryable<BudgetUser> query;
            switch (searchOperator)
            {
                case SearchOperator.Equals:
                    query = GetAll().Where(u => string.Equals(u.Username, searchText));
                    break;
                case SearchOperator.Contains:
                    query = GetAll().Where(u => u.Username.Contains(searchText));
                    break;
                case SearchOperator.StartsWith:
                    query = GetAll().Where(u => u.Username.StartsWith(searchText));
                    break;
                default:
                    throw new NotSupportedException();
            }

            var displayNameDefinitionId = Uow.DbContext.ProfilePropertyDefinition.FirstOrDefault(p => p.PropertyName == "DisplayName")?.PropertyDefinitionId;

            IQueryable<UserSearch> searchQueryable;
            if (!includePhoto)
            {
                searchQueryable = from match in query
                                  from displayNameUserProfile in Uow.DbContext.UserProfile.Where(up =>
                                          up.PropertyDefinitionId == displayNameDefinitionId &&
                                          up.UserId == match.UserId)
                                      .DefaultIfEmpty()
                                  select new UserSearch
                                  {
                                      UserId = match.UserId,
                                      DisplayName = displayNameUserProfile.PropertyValue,
                                      SearchFieldValue = match.Username
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
                                  select new UserSearch
                                  {
                                      UserId = match.UserId,
                                      Avatar = avatarUserProfile.PropertyValue,
                                      DisplayName = displayNameUserProfile.PropertyValue,
                                      SearchFieldValue = match.Username
                                  };
            }

            return searchQueryable;
        }

        public async override Task<BudgetUser> Create(BudgetUser entity)
        {
            var dbRecord = new Users
            {
                Username = entity.Username,
                CreatedOnDate = DateTime.UtcNow,
                Email = "",
                FirstName = "",
                LastName = "",
                DisplayName = ""
            };

            Uow.DbContext.Users.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<BudgetUser> Update(BudgetUser entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.Users
                                  where r.UserId == entity.UserId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no User with ID " + entity.UserId + ".");
            }

            // Update the database record.
            dbRecord.Username = entity.Username;
            dbRecord.Email = entity.Email;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public override Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        private BudgetUser Translate(Users dbRecord)
        {
            DateTime? defaultDate = null;
            var User = new BudgetUser
            {
                UserId = dbRecord.UserId,
                Username = dbRecord.Username,
                Email = dbRecord.Email,
                CreatedDate = dbRecord.CreatedOnDate.HasValue ? Convert.ToDateTime(dbRecord.CreatedOnDate) : defaultDate
            };

            return User;
        }
    }
}
