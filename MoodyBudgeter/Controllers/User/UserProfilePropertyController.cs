using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Lock;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.User
{
    [Route("user/v1/[controller]")]
    public class UserProfilePropertyController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly UserContextWrapper Context;

        public UserProfilePropertyController(IBudgeterCache cache, IBudgeterLock budgeterLock)
        {
            Cache = cache;
            BudgeterLock = budgeterLock;
            Context = new UserContextWrapper();
        }

        [HttpGet]
        [BudgeterAuthorize]
        public async Task<List<UserProfileProperty>> Get(int? userId)
        {
            CheckIfPassedUserIDAllowed(userId);

            var userProfilePropertyLogic = new UserProfilePropertyLogic(Cache, Context);

            return await userProfilePropertyLogic.GetUserProfileProperties(userId ?? UserId, IsAdmin);
        }

        [HttpGet, Route("{id}")]
        [BudgeterAuthorize]
        public async Task<UserProfileProperty> Get(int id)
        {
            var userProfilePropertyLogic = new UserProfilePropertyLogic(Cache, Context);

            var userProfileProperty = await userProfilePropertyLogic.GetUserProfileProperty(id, IsAdmin);

            if (userProfileProperty == null)
            {
                return null;
            }

            CheckIfPassedUserIDAllowed(userProfileProperty.UserId);

            return userProfileProperty;
        }

        [HttpPut]
        [BudgeterAuthorize]
        public async Task<List<UserProfileProperty>> Put([FromBody]List<UserProfileProperty> userProfileProperties)
        {
            CheckNullBody(userProfileProperties);

            if (!IsAdmin && userProfileProperties.Any(c => c.UserId != UserId))
            {
                throw new CallerException("Users cannot update other Users profiles.");
            }

            var userProfileUpdater = new UserProfileUpdater(Cache, BudgeterLock, Context);

            return await userProfileUpdater.UpdateUserProfileProperties(userProfileProperties, IsAdmin);
        }
    }
}
