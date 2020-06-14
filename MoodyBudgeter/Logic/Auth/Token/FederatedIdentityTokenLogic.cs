using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class FederatedIdentityTokenLogic
    {
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper UserContext;
        private readonly AuthContextWrapper AuthContext;

        public FederatedIdentityTokenLogic(IEnvironmentRequester environmentRequester, UserContextWrapper userContext, AuthContextWrapper authContext, IBudgeterCache cache)
        {
            EnvironmentRequester = environmentRequester;
            UserContext = userContext;
            AuthContext = authContext;
            Cache = cache;
        }

        // Right now no security roles will exist for a federated token.
        public async Task<BudgeterToken> IssueFederatedIdentityToken(int userId, string audience, string provider)
        {
            var tokenIssuer = new TokenIssuer(EnvironmentRequester, AuthContext, UserContext, Cache);

            var accessTokenData = await tokenIssuer.IssueToken(userId, audience, null, TokenType.Federated, provider);

            return new BudgeterToken
            {
                AccessToken = accessTokenData.Key,
                ExpiresIn = accessTokenData.Value
            };
        }
    }
}
