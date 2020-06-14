using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.Auth;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class UserSecurityRoleRepository : Repository<UserSecurityRole>
    {
        private readonly UnitOfWork Uow;

        public UserSecurityRoleRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public override Task<UserSecurityRole> Find(int id)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<UserSecurityRole> GetAll()
        {
            return (from r in Uow.DbContext.UserSecurityRole
                    select new UserSecurityRole
                    {
                        UserId = r.UserId,
                        SecurityRole = (SecurityRole)r.SecurityRoleId,
                        DateCreated = r.DateCreated,
                        CreatedBy = r.CreatedBy
                    });
        }

        public async override Task<UserSecurityRole> Create(UserSecurityRole entity)
        {
            var dbRecord = new AuthUserSecurityRole
            {
                UserId = entity.UserId,
                SecurityRoleId = (int)entity.SecurityRole,
                DateCreated = DateTime.UtcNow,
                CreatedBy = entity.CreatedBy
            };

            Uow.DbContext.UserSecurityRole.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public override Task<UserSecurityRole> Update(UserSecurityRole entity)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(int userId, SecurityRole securityRole)
        {
            var dbRecord = await (from r in Uow.DbContext.UserSecurityRole
                                  where r.UserId == userId
                                  && r.SecurityRoleId == (int)securityRole
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("User Security Role does not exist");
            }

            Uow.DbContext.UserSecurityRole.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private UserSecurityRole Translate(AuthUserSecurityRole dbRecord)
        {
            return new UserSecurityRole
            {
                UserId = dbRecord.UserId,
                SecurityRole = (SecurityRole)dbRecord.SecurityRoleId,
                DateCreated = dbRecord.DateCreated,
                CreatedBy = dbRecord.CreatedBy
            };
        }
    }
}
