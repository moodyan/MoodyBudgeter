using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Repositories.User.Roles;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Roles
{
    public class RoleLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public RoleLogic(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<List<Role>> GetRolesForPortal(bool isAdmin, int? roleGroupId)
        {
            var roleCache = new RoleCache(Cache);
            List<Role> roles = await roleCache.GetRolesFromCache(isAdmin, roleGroupId);

            if (roles != null)
            {
                return roles;
            }
            
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);
                var query = repo.GetAll();

                if (isAdmin)
                {
                    query = query.Where(r => r.IsVisible);
                }
                roles = await query.ToListAsync();
            }

            await roleCache.AddRolesToCache(roles, isAdmin, roleGroupId);

            return roles;
        }

        public async Task<List<Role>> GetAutoRolesOnPortal()
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                return await repo.GetAll().Where(c => c.AutoAssignment).ToListAsync();
            }
        }

        public async Task<Role> GetRole(int roleID, bool isAdmin)
        {
            Role role;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                role = isAdmin ? await repo.GetAll().Where(r => r.RoleId == roleID).FirstOrDefaultAsync() : await repo.GetAll().Where(r => r.RoleId == roleID && r.IsVisible).FirstOrDefaultAsync();
            }

            return role;
        }

        public async Task<Role> AddRole(Role role)
        {
            Role createdRole;

            ValidateRoleName(role.RoleId, role.RoleName);

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                createdRole = await repo.Create(role);
            }

            await new RoleCache(Cache).InvalidateRolesCache();
            return createdRole;
        }

        public async Task<Role> UpdateRole(Role role)
        {
            Role updatedRole;

            ValidateRoleName(role.RoleId, role.RoleName);

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                updatedRole = await repo.Update(role);
            }

            await new RoleCache(Cache).InvalidateRolesCache();
            await new UserRoleCache(Cache).InvalidateUserRoleCache();
            return updatedRole;
        }

        public async Task<Page<Role>> GetGrid(GridRequest gridRequest)
        {
            var data = new Page<Role>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                var query = repo.GetAllWithRelated();

                var dataGridLogic = new DataGridLogic<Role>(gridRequest, query);

                data.Records = await dataGridLogic.GetResults();
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }

        public async Task<Role> GetRoleFromName(string roleName)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                return await repo.GetAll().Where(r => r.RoleName.ToUpperInvariant() == roleName.ToUpperInvariant()).FirstOrDefaultAsync();
            }
        }

        private void ValidateRoleName(int roleId, string newName)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new RoleRepository(uow);

                if (repo.GetAll().Any(r => r.RoleId != roleId && r.RoleName == newName))
                {
                    throw new FriendlyException("Role.NameAlreadyExists", "A role with the specified name already exists");
                }
            }
        }
    }
}