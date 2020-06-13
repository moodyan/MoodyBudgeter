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

        public GoogleAuthController(IGoogleOAuthClient googleOAuthClient, ILoyaltyRequester loyaltyRequester, IRestRequester restRequester, IEnvironmentRequester environmentRequester, IConfigurationRequester configurationRequester)
        {
            GoogleOAuthClient = googleOAuthClient;
            LoyaltyRequester = loyaltyRequester;
            RestRequester = restRequester;
            EnvironmentRequester = environmentRequester;
            ConfigurationRequester = configurationRequester;
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
        public async Task<GoogleSSOResponse> Login([FromBody] SSORequest ssoRequest)
        {
            CheckNullBody(ssoRequest);

            if (string.IsNullOrEmpty(ssoRequest.ClientId) || string.IsNullOrEmpty(ssoRequest.AuthorizationCode))
            {
                throw new CallerException("Google Client Id and Authorization Code are required.");
            }

            SSOLogic ssoLogic = new SSOLogic(GoogleOAuthClient, LoyaltyRequester, RestRequester, EnvironmentRequester, ConfigurationRequester);

            GoogleTokenResponse tokenResponse = await ssoLogic.VerifyAuthCode(ssoRequest, PortalId);

            return await ssoLogic.LoginOrRegisterGoogleUser(tokenResponse, PortalId);
        }
    }
}
