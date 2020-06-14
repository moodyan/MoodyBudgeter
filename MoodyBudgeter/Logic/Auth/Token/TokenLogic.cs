using MoodyBudgeter.Logic.Auth.App;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class TokenLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly Repositories.Auth.ContextWrapper AuthContext;
        private readonly Repositories.User.ContextWrapper UserContext;
        private readonly string AuthHeader;
        private readonly TokenRequest TokenRequest;

        public TokenLogic(IEnvironmentRequester environmentRequester, IBudgeterCache cache, Repositories.Auth.ContextWrapper authContext, Repositories.User.ContextWrapper userContext, string authHeader, TokenRequest tokenRequest)
        {
            EnvironmentRequester = environmentRequester;
            Cache = cache;
            AuthContext = authContext;
            UserContext = userContext;
            AuthHeader = authHeader;
            TokenRequest = tokenRequest;
        }

        public async Task<BudgeterToken> GetToken()
        {
            BudgeterToken token = null;

            if (string.IsNullOrEmpty(TokenRequest.GrantType))
            {
                throw new CallerException("Invalid grant type");
            }

            switch (TokenRequest.GrantType.ToLower())
            {
                case "authorization_code":
                    token = await GetTokenFromAuthCode();
                    break;
                case "refresh_token":
                    token = await GetTokenFromRefreshToken();
                    break;
                case "client_credentials":
                    token = await GetTokenFromClientCredentials();
                    break;
                default:
                    throw new CallerException("Invalid grant type");
            }

            return token;
        }

        private async Task<BudgeterToken> GetTokenFromAuthCode()
        {
            if (string.IsNullOrEmpty(TokenRequest.ClientId) || string.IsNullOrEmpty(TokenRequest.Code) || string.IsNullOrEmpty(TokenRequest.CodeVerifier) || string.IsNullOrEmpty(TokenRequest.RedirectUri))
            {
                throw new CallerException("Missing fields for a authorization_code grant.");
            }

            await new AppValidator(Cache, AuthContext).ValidateAuthCodeTokenRequest(TokenRequest, AuthHeader);

            int userId = await new AuthCodeLogic(Cache).ValidateCode(TokenRequest);

            var securityRoles = await new UserSecurityRoleLogic(AuthContext).GetSecurityRoles(userId, true);

            var tokenIssuer = new TokenIssuer(EnvironmentRequester, AuthContext, UserContext, Cache);

            var accessTokenData = await tokenIssuer.IssueToken(userId, TokenRequest.ClientId, securityRoles, TokenType.AuthCode);

            var refreshTokenData = await tokenIssuer.IssueToken(userId, TokenRequest.ClientId, null, TokenType.Refresh);

            return new BudgeterToken
            {
                AccessToken = accessTokenData.Key,
                RefreshToken = refreshTokenData.Key,
                ExpiresIn = accessTokenData.Value
            };
        }

        private async Task<BudgeterToken> GetTokenFromRefreshToken()
        {
            if (string.IsNullOrEmpty(TokenRequest.ClientId) || string.IsNullOrEmpty(TokenRequest.RefreshToken))
            {
                throw new CallerException("Missing fields for a refresh_token request.");
            }

            await new AppValidator(Cache, AuthContext).ValidateRefreshTokenRequest(TokenRequest, AuthHeader);

            var tokenValidator = new TokenValidator(EnvironmentRequester);

            tokenValidator.ValidateToken(TokenRequest.RefreshToken, TokenType.Refresh);

            int userId = tokenValidator.UserId;

            var securityRoles = await new UserSecurityRoleLogic(AuthContext).GetSecurityRoles(userId, true);

            var accessTokenData = await new TokenIssuer(EnvironmentRequester, AuthContext, UserContext, Cache).IssueToken(userId, TokenRequest.ClientId, securityRoles, TokenType.RefreshAccess);

            return new BudgeterToken
            {
                AccessToken = accessTokenData.Key,
                ExpiresIn = accessTokenData.Value
            };
        }

        private async Task<BudgeterToken> GetTokenFromClientCredentials()
        {
            if (string.IsNullOrEmpty(TokenRequest.ClientId))
            {
                throw new CallerException("Missing fields for a client_credentials request.");
            }

            var app = await new AppValidator(Cache, AuthContext).ValidateClientCredentialsTokenRequest(TokenRequest, AuthHeader);

            var securityRoles = await new UserSecurityRoleLogic(AuthContext).GetSecurityRoles(app.UserId.Value, true);

            var accessTokenData = await new TokenIssuer(EnvironmentRequester, AuthContext, UserContext, Cache).IssueToken(app.UserId.Value, TokenRequest.ClientId, securityRoles, TokenType.ClientCredentials, checkEnabled: !TokenRequest.SuperUserRequest);

            return new BudgeterToken
            {
                AccessToken = accessTokenData.Key,
                ExpiresIn = accessTokenData.Value
            };
        }
    }
}
