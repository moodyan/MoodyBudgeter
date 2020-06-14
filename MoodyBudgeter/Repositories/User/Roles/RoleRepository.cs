using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User.Roles
{
    public class RoleRepository : Repository<Role>
    {
        private readonly UnitOfWork Uow;

        public RoleRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<Role> Find(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.Roles
                                  where r.RoleId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new ArgumentException("There is no Role with ID " + id + ".");
            }

            return Translate(dbRecord);
        }

        public override IQueryable<Role> GetAll()
        {
            return (from r in Uow.DbContext.Roles
                    select new Role
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        IsVisible = r.IsPublic,
                        AutoAssignment = r.AutoAssignment,
                        Ordinal = r.Ordinal
                    });
        }

        public IQueryable<Role> GetAllWithRelated()
        {
            // Get the records from the database as a IQueryable.
            return (from r in Uow.DbContext.Roles
                    select new Role
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        IsVisible = r.IsPublic,
                        AutoAssignment = r.AutoAssignment,
                        Ordinal = r.Ordinal
                    });
        }

        public async override Task<Role> Create(Role entity)
        {
            var dbRecord = new Data.User.Roles
            {
                RoleId = entity.RoleId,
                RoleName = entity.RoleName,
                Description = entity.Description,
                IsPublic = entity.IsVisible,
                AutoAssignment = entity.AutoAssignment,
                Ordinal = entity.Ordinal
            };

            Uow.DbContext.Roles.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<Role> Update(Role entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = await (from r in Uow.DbContext.Roles
                                  where r.RoleId == entity.RoleId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no Role with ID " + entity.RoleId + ".");
            }

            // Update the database record.
            dbRecord.RoleId = entity.RoleId;
            dbRecord.RoleName = entity.RoleName;
            dbRecord.Description = entity.Description;
            dbRecord.IsPublic = entity.IsVisible;
            dbRecord.AutoAssignment = entity.AutoAssignment;
            dbRecord.Ordinal = entity.Ordinal;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.Roles
                                  where r.RoleId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("Role does not exist.");
            }

            Uow.DbContext.Roles.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private Role Translate(Data.User.Roles dbRecord)
        {
            var Role = new Role
            {
                RoleId = dbRecord.RoleId,
                RoleName = dbRecord.RoleName,
                Description = dbRecord.Description,
                IsVisible = dbRecord.IsPublic,
                AutoAssignment = dbRecord.AutoAssignment,
                Ordinal = dbRecord.Ordinal
            };

            return Role;
        }
    }
}
