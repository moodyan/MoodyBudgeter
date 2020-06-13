using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.User;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User
{
    public class UserRoleRepository : Repository<UserRole>
    {
        private readonly UnitOfWork Uow;

        public UserRoleRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<UserRole> Find(int id)
        {
            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.UserRoles
                                  where r.UserRoleId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no UserRole with ID " + id + ".");
            }

            // Return the response data.
            return Translate(dbRecord);
        }

        public override IQueryable<UserRole> GetAll()
        {
            return (from r in Uow.DbContext.UserRoles
                    select new UserRole
                    {
                        UserRoleId = r.UserRoleId,
                        UserId = r.UserId,
                        RoleId = r.RoleId,
                        ExpiryDate = r.ExpiryDate,
                        CreatedOnDate = r.CreatedOnDate
                    });
        }

        public IQueryable<UserRole> GetAllWithRelated(bool isAdmin, bool enforceExpirationDate = true)
        {
            IQueryable<UserRoles> query = null;

            if (enforceExpirationDate)
            {
                query = Uow.DbContext.UserRoles.Where(r => !r.ExpiryDate.HasValue || r.ExpiryDate > DateTime.UtcNow);
            }

            if (!isAdmin)
            {
                query = Uow.DbContext.UserRoles.Where(r => r.Role.IsPublic);
            }

            return query.Select(r => new UserRole()
            {
                UserRoleId = r.UserRoleId,
                UserId = r.UserId,
                RoleId = r.RoleId,
                ExpiryDate = r.ExpiryDate,
                CreatedOnDate = r.CreatedOnDate,
                RoleName = r.Role.RoleName,
                RoleGroupId = r.Role.RoleGroupId,
                RoleGroupName = r.Role.RoleGroup == null ? null : r.Role.RoleGroup.RoleGroupName,
                Ordinal = r.Role.Ordinal
            });
        }

        public IQueryable<UserRoleGrid> GetAllForGrid(int? userId, int? roleId)
        {
            IQueryable<UserRoles> query;

            if (userId.HasValue)
            {
                query = Uow.DbContext.UserRoles.Where(r => r.UserId == userId);
            }
            else
            {
                query = Uow.DbContext.UserRoles.Where(r => r.RoleId == roleId);
            }

            return (from u in query
                    select new UserRoleGrid
                    {
                        UserRoleId = u.UserRoleId,
                        UserId = u.UserId,
                        RoleId = u.RoleId,
                        ExpiryDate = u.ExpiryDate,
                        CreatedOnDate = u.CreatedOnDate,
                        RoleName = u.Role.RoleName,
                        RoleGroupName = u.Role.RoleGroup.RoleGroupName,
                        RoleGroupId = u.Role.RoleGroupId,
                        Ordinal = u.Role.Ordinal,
                        Username = u.User.Username,
                        Email = u.User.UserProfile.FirstOrDefault(p => p.PropertyDefinition.PropertyName == "Email").PropertyValue,
                        FirstName = u.User.UserProfile.FirstOrDefault(p => p.PropertyDefinition.PropertyName == "FirstName").PropertyValue,
                        LastName = u.User.UserProfile.FirstOrDefault(p => p.PropertyDefinition.PropertyName == "LastName").PropertyValue
                    });
        }

        public async override Task<UserRole> Create(UserRole entity)
        {
            var dbRecord = new UserRoles
            {
                UserId = entity.UserId,
                RoleId = entity.RoleId,
                ExpiryDate = entity.ExpiryDate,
                CreatedOnDate = DateTime.UtcNow,
                LastModifiedOnDate = DateTime.UtcNow,
                Status = 1,
            };

            Uow.DbContext.UserRoles.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<UserRole> Update(UserRole entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.UserRoles
                                  where r.UserRoleId == entity.UserRoleId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no UserRole with ID " + entity.UserRoleId + ".");
            }

            // Update the database record.
            dbRecord.UserId = entity.UserId;
            dbRecord.RoleId = entity.RoleId;
            dbRecord.LastModifiedOnDate = DateTime.UtcNow;
            dbRecord.ExpiryDate = entity.ExpiryDate;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.UserRoles
                                  where r.UserRoleId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("UserRole does not exist.");
            }

            Uow.DbContext.UserRoles.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private UserRole Translate(UserRoles dbRecord)
        {
            var UserRole = new UserRole
            {
                UserRoleId = dbRecord.UserRoleId,
                UserId = dbRecord.UserId,
                RoleId = dbRecord.RoleId,
                ExpiryDate = dbRecord.ExpiryDate,
                CreatedOnDate = dbRecord.CreatedOnDate,
            };

            return UserRole;
        }
    }
}
