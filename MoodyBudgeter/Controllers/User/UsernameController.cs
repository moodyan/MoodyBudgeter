using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.Settings;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.User
{
    [Route("user/v1/user/{userid}/[controller]")]
    public class UsernameController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly ISettingRequester SettingRequester;
        private readonly UserContextWrapper Context;

        public UsernameController(IBudgeterCache cache, ISettingRequester settingRequester)
        {
            Cache = cache;
            SettingRequester = settingRequester;
            Context = new UserContextWrapper();
        }

        [HttpPut]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<BudgetUser> Put(int userid, string proposedUsername)
        {
            var usernameLogic = new UsernameLogic(Cache, SettingRequester, Context);

            return await usernameLogic.UpdateUsername(userid, proposedUsername);
        }
    }
}