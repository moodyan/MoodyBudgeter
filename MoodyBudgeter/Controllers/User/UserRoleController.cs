using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Logic.User.Roles;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.User
{
    [Route("user/v1/[controller]")]
    public class UserRoleController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public UserRoleController(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new UserContextWrapper();
        }

        [HttpGet]
        [BudgeterAuthorize]
        public async Task<List<UserRole>> Get(int? userId)
        {
            CheckIfPassedUserIDAllowed(userId);

            var userRoleLogic = new UserRoleLogic(Cache, Context);

            return await userRoleLogic.GetUserRoles(userId ?? UserId, IsAdmin);
        }

        [HttpGet, Route("{id}")]
        [BudgeterAuthorize]
        public async Task<UserRole> Get(int id)
        {
            var userRoleLogic = new UserRoleLogic(Cache, Context);

            var userRole = await userRoleLogic.GetUserRole(id, IsAdmin);

            if (userRole == null)
            {
                return null;
            }

            CheckIfPassedUserIDAllowed(userRole.UserId);

            return userRole;
        }

        [HttpPost]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<UserRole> Post([FromBody]UserRole userRole)
        {
            var userRoleUpdater = new UserRoleUpdater(Cache, Context);

            return await userRoleUpdater.AddRoleToUser(userRole);
        }

        [HttpDelete]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task Delete(int userId, int roleId)
        {
            var userRoleUpdater = new UserRoleUpdater(Cache, Context);

            await userRoleUpdater.RemoveRoleFromUser(userId, roleId);
        }

        [HttpDelete, Route("{id}")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task Delete(int id)
        {
            var userRoleUpdater = new UserRoleUpdater(Cache, Context);

            await userRoleUpdater.RemoveRoleFromUser(id);
        }

        [HttpPost, Route("grid")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Page<UserRoleGrid>> Grid([FromBody]GridRequest gridRequest, int? userId = null, int? roleId = null)
        {
            CheckNullBody(gridRequest);

            var userRoleLogic = new UserRoleLogic(Cache, Context);

            return await userRoleLogic.GetGrid(gridRequest, userId, roleId);
        }
    }
}
