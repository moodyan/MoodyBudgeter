using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Token;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.Auth
{
    [Route("auth")]
    public class TokenController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly AuthContextWrapper AuthContext;
        private readonly UserContextWrapper UserContext;

        public TokenController(IBudgeterCache cache, IEnvironmentRequester environmentRequester, AuthContextWrapper authContext, UserContextWrapper userContext)
        {
            Cache = cache;
            EnvironmentRequester = environmentRequester;
            AuthContext = authContext;
            UserContext = userContext;
        }

        [HttpPost, Route("v1/[controller]")]
        public async Task<BudgeterToken> Post([FromBody]TokenRequest tokenRequest)
        {
            CheckNullBody(tokenRequest);

            string authHeader = Request.Headers.Where(c => c.Key == "Authorization").Select(c => c.Value).FirstOrDefault();

            var tokenLogic = new TokenLogic(EnvironmentRequester, Cache, AuthContext, UserContext, authHeader, tokenRequest);

            return await tokenLogic.GetToken();
        }

        [HttpPost, Route("v1/[controller]")]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task<BudgeterToken> SuperUserToken([FromBody]TokenRequest tokenRequest)
        {
            CheckNullBody(tokenRequest);

            string authHeader = Request.Headers.Where(c => c.Key == "Authorization").Select(c => c.Value).FirstOrDefault();

            var tokenLogic = new TokenLogic(EnvironmentRequester, Cache, AuthContext, UserContext, authHeader, tokenRequest);

            return await tokenLogic.GetToken();
        }

        [HttpGet, Route("v1/[controller]/provider")]
        [BudgeterAuthorize((int)SecurityRole.SuperUser)]
        public async Task<BudgeterToken> Provider(int userId, string audience, string provider)
        {
            var providerTokenLogic = new FederatedIdentityTokenLogic(EnvironmentRequester, UserContext, AuthContext, Cache);

            return await providerTokenLogic.IssueFederatedIdentityToken(userId, audience, provider);
        }
    }
}
