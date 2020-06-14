using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth;
using MoodyBudgeter.Logic.Auth.Password;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.Settings;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.Auth
{
    [Route("auth/v1/[controller]")]
    public class UserLoginController : BudgeterBaseController
    {
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly ISettingRequester SettingsRequester;
        private readonly AuthContextWrapper Context;

        public UserLoginController(IEnvironmentRequester environmentRequester, ISettingRequester settingsRequester, AuthContextWrapper context)
        {
            EnvironmentRequester = environmentRequester;
            SettingsRequester = settingsRequester;
            Context = context;
        }

        [HttpGet]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<UserCredential> Get(int userId)
        {
            var userLoginLogic = new UserLoginLogic(Context);

            return await userLoginLogic.GetUserLogin(userId);
        }

        [HttpPost]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<UserCredential> Post([FromBody]UserCredential userLogin)
        {
            var userLoginLogic = new UserLoginLogic(Context);

            return await userLoginLogic.CreateLogin(userLogin);
        }

        [HttpPost, Route("unlock")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task Unlock(int userId)
        {
            var passwordLogic = new PasswordLogic(Context);

            await passwordLogic.ResetAttemptCount(userId);
        }
    }
}
