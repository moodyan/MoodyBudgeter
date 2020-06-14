using Microsoft.IdentityModel.Tokens;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class TokenIssuer
    {
        private readonly Repositories.Auth.ContextWrapper AuthContext;
        private readonly Repositories.User.ContextWrapper UserContext;
        private readonly IBudgeterCache Cache;
        private readonly string SecretKey;
        private readonly string Issuer;

        private int ExpirationDateInSeconds;
        private DateTime ExpirationDate;

        public TokenIssuer(IEnvironmentRequester environmentRequester, Repositories.Auth.ContextWrapper authContext, Repositories.User.ContextWrapper userContext, IBudgeterCache cache)
        {
            AuthContext = authContext;
            UserContext = userContext;
            Cache = cache;

            SecretKey = environmentRequester.GetVariable("TokenSecretKey");
            Issuer = environmentRequester.GetVariable("Issuer");
        }

        public async Task<KeyValuePair<string, int>> IssueToken(int userId, string audience, List<SecurityRole> roles, TokenType type, string provider = "budgeter", bool checkEnabled = true)
        {
            if (checkEnabled)
            {
                var userLogic = new UserLogic(Cache, UserContext);
                var user = await userLogic.GetUserWithoutRelated(userId);

                if (user == null || !user.Enabled)
                {
                    throw new FriendlyException("IssueToken.UserNotEnabled", "User is not enabled");
                }
            }

            GetExpiration(type);

            await LoginActions(userId, audience, type, provider);

            var claims = GetClaims(userId, type, provider);

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim("rol", role.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: audience,
                claims: claims,
                expires: ExpirationDate,
                signingCredentials: creds);

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new KeyValuePair<string, int>(accessToken, ExpirationDateInSeconds);
        }

        private List<Claim> GetClaims(int userId, TokenType type, string provider)
        {
            var claims = new List<Claim>
            {
                new Claim("uid", userId.ToString(), ClaimValueTypes.Integer),
                new Claim("typ", type.ToString().ToLower()),
                new Claim("iat", ((int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(), ClaimValueTypes.Integer),
                new Claim("prv", provider)
            };

            return claims;
        }

        private async Task LoginActions(int userId, string audience, TokenType type, string provider)
        {
            if (type == TokenType.Refresh || type == TokenType.Identity)
            {
                return;
            }

            var loginHistoryLogic = new UserLoginHistoryLogic(AuthContext);

            await loginHistoryLogic.RecordUserLogin(userId, type, provider, audience);

            //await SendUserLoginEvent(userId);
        }

        //private async Task SendUserLoginEvent(int userId)
        //{
        //    var message = new UserLoginEvent
        //    {
        //        UserId = userId
        //    };

        //    await QueueSender.SendMessage<UserLoginEvent>(message);
        //}

        private void GetExpiration(TokenType type)
        {
            if (type == TokenType.Refresh || type == TokenType.Identity)
            {
                ExpirationDate = DateTime.UtcNow.AddDays(30);
                ExpirationDateInSeconds = -1;
                return;
            }

            ExpirationDate = DateTime.UtcNow.AddHours(4);
            ExpirationDateInSeconds = 14400;
        }
    }
}
