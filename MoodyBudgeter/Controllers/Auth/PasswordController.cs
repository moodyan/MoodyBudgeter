using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Password;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.RestRequester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.Auth
{
    [Route("auth/{portalid}/v1/[controller]")]
    public class PasswordController : BaseController
    {
        private readonly IRestRequester RestRequester;
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly ContextWrapper Context;
        //private readonly IBudgeterCache Cache; ////TODO Add Caching

        public PasswordController(IRestRequester restRequester, IEnvironmentRequester environmentRequester)
        {
            RestRequester = restRequester;
            EnvironmentRequester = environmentRequester;
            Context = new ContextWrapper();
            //Cache = cache;
        }

        [HttpPut]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task Put(int userId, string previousPassword, string proposedPassword)
        {
            await new PasswordLogic(Context).ChangePassword(userId, previousPassword, proposedPassword);
        }

        [HttpPost, Route("reset")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task Reset(int userId)
        {
            var passwordResetLogic = new PasswordResetLogic(Context);

            await passwordResetLogic.ResetPassword(userId);
        }

        [HttpPost, Route("reset/emptycredential")]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task<ActionResult> Reset(int userId, string username)
        {
            var passwordResetLogic = new PasswordResetLogic(Context);

            string token = await passwordResetLogic.CreateEmptyCredentialsWithResetToken(userId, username);

            return Json(token);
        }
    }
}
