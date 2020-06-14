using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Google;
using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.Settings;
using MoodyBudgeter.Utility.Lock;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers
{

    [Route("google/v1/[controller]")]
    public class GoogleAuthController : BudgeterBaseController
    {
        private readonly IGoogleOAuthClient GoogleOAuthClient;
        private readonly ISettingRequester SettingRequester;
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly UserContextWrapper UserContext;
        private readonly AuthContextWrapper AuthContext;

        public GoogleAuthController(IGoogleOAuthClient googleOAuthClient, ISettingRequester settingRequester, IEnvironmentRequester environmentRequester, IBudgeterCache cache, IBudgeterLock budgeterLock)
        {
            GoogleOAuthClient = googleOAuthClient;
            SettingRequester = settingRequester;
            EnvironmentRequester = environmentRequester;
            Cache = cache;
            UserContext = new UserContextWrapper();
            AuthContext = new AuthContextWrapper();
            BudgeterLock = budgeterLock;
        }

        /// <summary>
        /// Logs a user into Loyalty with a Google Access Token
        /// </summary>
        /// <remarks>
        /// Given a Google One-Time Auth Code and ClientId, grant a Loyalty Access token for that user if they can be found in Loyalty based on the Google Email associated with the Access Token
        /// </remarks>
        /// <param name="ssoRequest">The SSO Request body</param>
        /// <returns></returns>
        [HttpPost, Route("login")]
        public async Task<GoogleSSOResponse> Login([FromBody] GoogleSSORequest ssoRequest)
        {
            CheckNullBody(ssoRequest);

            if (string.IsNullOrEmpty(ssoRequest.ClientId) || string.IsNullOrEmpty(ssoRequest.AuthorizationCode))
            {
                throw new CallerException("Google Client Id and Authorization Code are required.");
            }

            GoogleSSOLogic ssoLogic = new GoogleSSOLogic(GoogleOAuthClient, EnvironmentRequester, SettingRequester, Cache, UserContext, AuthContext, BudgeterLock);

            GoogleTokenResponse tokenResponse = await ssoLogic.VerifyAuthCode(ssoRequest);

            return await ssoLogic.LoginOrRegisterGoogleUser(tokenResponse, IsAdmin);
        }
    }
}
