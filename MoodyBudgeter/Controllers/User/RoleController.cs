using Microsoft.AspNetCore.Mvc;
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
    public class RoleController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public RoleController(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new UserContextWrapper();
        }

        [HttpGet]
        [BudgeterAuthorize]
        public async Task<List<Role>> GetRoles()
        {
            var roleLogic = new RoleLogic(Cache, Context);

            return await roleLogic.GetRoles(IsAdmin);
        }

        [HttpGet, Route("{id}")]
        [BudgeterAuthorize]
        public async Task<Role> Get(int id)
        {
            var roleLogic = new RoleLogic(Cache, Context);

            return await roleLogic.GetRole(id, IsAdmin);
        }

        [HttpPut, Route("{id}")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Role> Put(int id, [FromBody]Role role)
        {
            CheckNullBody(role);

            role.RoleId = id;

            var roleLogic = new RoleLogic(Cache, Context);

            return await roleLogic.UpdateRole(role);
        }

        [HttpPost]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Role> Post([FromBody]Role role)
        {
            CheckNullBody(role);

            var roleLogic = new RoleLogic(Cache, Context);

            return await roleLogic.AddRole(role);
        }

        [HttpPost, Route("grid")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Page<Role>> Grid([FromBody]GridRequest gridRequest)
        {
            CheckNullBody(gridRequest);

            var roleLogic = new RoleLogic(Cache, Context);

            return await roleLogic.GetGrid(gridRequest);
        }
    }
}
