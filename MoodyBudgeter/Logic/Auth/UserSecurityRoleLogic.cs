using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth
{
    public class UserSecurityRoleLogic
    {
        private readonly AuthContextWrapper Context;

        public UserSecurityRoleLogic(AuthContextWrapper context)
        {
            Context = context;
        }

        public async Task<List<SecurityRole>> GetSecurityRoles(int userId)
        {
            var userLogin = await new UserLoginLogic(Context).GetUserLogin(userId);

            if (userLogin == null)
            {
                throw new CallerException("User does not have login");
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserSecurityRoleRepository(uow);

                var securityRoles = await repo.GetAll().Where(c => c.UserId == userId).ToListAsync();

                return securityRoles.Select(c => c.SecurityRole).ToList();
            }
        }

        public async Task AddSecurityRole(int userId, SecurityRole role, string createdBy)
        {
            if (userId <= 0)
            {
                throw new CallerException("Invalid UserId");
            }

            if (role == 0)
            {
                throw new CallerException("Invalid Role");
            }

            if (string.IsNullOrEmpty(createdBy))
            {
                throw new CallerException("CreatedBy required.");
            }

            var userLogin = await new UserLoginLogic(Context).GetUserLogin(userId);

            if (userLogin == null)
            {
                throw new CallerException("User does not have login");
            }

            var currentRoles = await GetSecurityRoles(userId);

            if (currentRoles.Contains(role))
            {
                throw new CallerException("User already has role");
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserSecurityRoleRepository(uow);

                await repo.Create(new UserSecurityRole
                {
                    UserId = userId,
                    SecurityRole = role,
                    CreatedBy = createdBy
                });
            }
        }

        public async Task RemoveSecurityRole(int userId, SecurityRole role)
        {
            var userLogin = await new UserLoginLogic(Context).GetUserLogin(userId);

            if (userLogin == null)
            {
                throw new CallerException("User does not have login");
            }

            if (role == 0)
            {
                throw new CallerException("Invalid Role");
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserSecurityRoleRepository(uow);

                await repo.Delete(userId, role);
            }
        }

        public async Task MakeSuperUser(int userId)
        {
            var roles = await GetSecurityRoles(userId);

            if (!roles.Contains(SecurityRole.Admin))
            {
                await AddSecurityRole(userId, SecurityRole.Admin, "MakeSuperUser");
            }

            if (!roles.Contains(SecurityRole.SuperUser))
            {
                await AddSecurityRole(userId, SecurityRole.SuperUser, "MakeSuperUser");
            }
        }
    }
}
