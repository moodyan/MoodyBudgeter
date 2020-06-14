using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.User
{
    [Route("user/v1/[controller]")]
    public class ProfilePropertyController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public ProfilePropertyController(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new UserContextWrapper();
        }

        [HttpGet]
        public async Task<List<ProfileProperty>> Get(bool requiredOnly)
        {
            var profilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            return await profilePropertyLogic.GetProfileProperties(requiredOnly, IsAdmin);
        }

        [HttpGet, Route("{id}")]
        public async Task<ProfileProperty> Get(int id)
        {
            var profilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            return await profilePropertyLogic.GetProfileProperty(id, IsAdmin);
        }

        [HttpPost]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<ProfileProperty> Post([FromBody]ProfileProperty profileProperty)
        {
            CheckNullBody(profileProperty);

            profileProperty.UpdatedBy = UserId;

            var ProfilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            return await ProfilePropertyLogic.AddProfileProperty(profileProperty);
        }

        [HttpPut, Route("{id}")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<ProfileProperty> Put(int id, [FromBody]ProfileProperty profileProperty)
        {
            CheckNullBody(profileProperty);

            profileProperty.ProfilePropertyId = id;
            profileProperty.UpdatedBy = UserId;

            var ProfilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            return await ProfilePropertyLogic.UpdateProfileProperty(profileProperty);
        }

        [HttpPost, Route("grid")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Page<ProfileProperty>> Grid([FromBody]GridRequest gridRequest)
        {
            CheckNullBody(gridRequest);

            var ProfilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            return await ProfilePropertyLogic.GetGrid(gridRequest);
        }
    }
}
