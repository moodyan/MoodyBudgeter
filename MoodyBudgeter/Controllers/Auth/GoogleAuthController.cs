using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Google;
using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.RestRequester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers
{

    [Route("google/{portalId}/v1/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IGoogleOAuthClient GoogleOAuthClient;
        private readonly IRestRequester RestRequester;
        private readonly IEnvironmentRequester EnvironmentRequester;

        public GoogleAuthController(IGoogleOAuthClient googleOAuthClient, IRestRequester restRequester, IEnvironmentRequester environmentRequester)
        {
            GoogleOAuthClient = googleOAuthClient;
            RestRequester = restRequester;
            EnvironmentRequester = environmentRequester;
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

            GoogleSSOLogic ssoLogic = new GoogleSSOLogic(GoogleOAuthClient, RestRequester, EnvironmentRequester);

            GoogleTokenResponse tokenResponse = await ssoLogic.VerifyAuthCode(ssoRequest);

            return await ssoLogic.LoginOrRegisterGoogleUser(tokenResponse);
        }
    }
}
