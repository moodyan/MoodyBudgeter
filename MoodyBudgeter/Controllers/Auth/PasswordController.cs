using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Password;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.Auth
{
    [Route("auth/v1/[controller]")]
    public class PasswordController : BudgeterBaseController
    {
        private readonly AuthContextWrapper AuthContext;
        private readonly UserContextWrapper UserContext;
        private readonly IBudgeterCache Cache;

        public PasswordController(IBudgeterCache cache)
        {
            UserContext = new UserContextWrapper();
            AuthContext = new AuthContextWrapper();
            Cache = cache;
        }

        [HttpPut]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task Put(int userId, string previousPassword, string proposedPassword)
        {
            await new PasswordLogic(AuthContext).ChangePassword(userId, previousPassword, proposedPassword);
        }

        [HttpPost, Route("reset")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task Reset(int userId)
        {
            var passwordResetLogic = new PasswordResetLogic(Cache, AuthContext, UserContext);

            await passwordResetLogic.ResetPassword(userId);
        }

        [HttpPost, Route("reset/emptycredential")]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task<ActionResult> Reset(int userId, string username)
        {
            var passwordResetLogic = new PasswordResetLogic(Cache, AuthContext, UserContext);

            string token = await passwordResetLogic.CreateEmptyCredentialsWithResetToken(userId, username);

            return Json(token);
        }
    }
}
