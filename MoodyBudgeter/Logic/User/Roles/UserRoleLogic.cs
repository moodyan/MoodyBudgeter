using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Logic.User.Roles;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UserRoleLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public UserRoleLogic(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<List<UserRole>> GetUserRoles(int userId, bool isAdmin)
        {
            var userRoleCache = new UserRoleCache(Cache);

            List<UserRole> cacheResult = await userRoleCache.GetUserRolesFromCache(userId, isAdmin);

            if (cacheResult != null)
            {
                return cacheResult;
            }

            List<UserRole> roles;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                roles = await repo.GetAllWithRelated(isAdmin).Where(c => c.UserId == userId).ToListAsync();
            }

            if (roles != null)
            {
                await userRoleCache.AddUserRolesToCache(userId, roles, isAdmin);
            }

            return roles;
        }

        public async Task<UserRole> GetUserRole(int userRoleId, bool isAdmin)
        {
            UserRole userRole;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                userRole = await repo.GetAllWithRelated(isAdmin).Where(ur => ur.UserRoleId == userRoleId).FirstOrDefaultAsync();
            }

            return userRole;
        }

        public async Task<UserRole> GetUserRole(int roleId, int userId)
        {
            UserRole userRole;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                userRole = await repo.GetAll().Where(r => r.RoleId == roleId && r.UserId == userId).FirstOrDefaultAsync();
            }

            return userRole;
        }

        public async Task<Page<UserRoleGrid>> GetGrid(GridRequest gridRequest, int? userId, int? roleId)
        {
            if (!userId.HasValue && !roleId.HasValue)
            {
                throw new CallerException("UserId or RoleId is required");
            }

            var data = new Page<UserRoleGrid>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                var query = repo.GetAllForGrid(userId, roleId);

                var dataGridLogic = new DataGridLogic<UserRoleGrid>(gridRequest, query);

                data.Records = await dataGridLogic.GetResults(); //TODO this wont work
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }
    }
}
