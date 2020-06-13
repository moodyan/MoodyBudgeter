using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class TokenValidator
    {
        private readonly string SecretKey;
        private readonly string Issuer;
        public int UserId { get; set; }
        public List<SecurityRole> SecurityRoles { get; set; }

        public TokenValidator(IEnvironmentRequester environmentRequester)
        {
            SecretKey = environmentRequester.GetVariable("TokenSecretKey");
            Issuer = environmentRequester.GetVariable("Issuer");
        }
        
        public void HandleToken(HttpRequest request, int portalId)
        {
            if (!request.Headers.TryGetValue("Authorization", out var authorizationValue))
            {
                return;
            }

            var authValue = authorizationValue.FirstOrDefault();

            if (authValue.StartsWith("Basic "))
            {
                return;
            }

            ValidateToken(authValue);
        }

        public void ValidateToken(string strToken)
        {
            strToken = strToken.Replace("Bearer ", "");

            JwtSecurityToken token;
            int tokenUserId = -1;
            string tokenType = "";

            SecurityRoles = new List<SecurityRole>();

            try
            {
                token = new JwtSecurityToken(strToken);

                foreach (var claim in token.Claims)
                {
                    if (claim.Type == "uid")
                    {
                        tokenUserId = int.Parse(claim.Value);
                    }

                    if (claim.Type == "typ")
                    {
                        tokenType = claim.Value;
                    }

                    if (claim.Type == "rol" && Enum.TryParse(claim.Value, out SecurityRole role))
                    {
                        SecurityRoles.Add(role);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Error parsing token", ex);
            }

            if (tokenUserId == -1)
            {
                throw new UnauthorizedAccessException("UserId missing from token");
            }

            if (string.IsNullOrEmpty(tokenType))
            {
                throw new UnauthorizedAccessException("Token does not have a type");
            }

            switch (tokenType)
            {
                case "implicit":
                    break;
                case "authcode":
                    break;
                case "refreshaccess":
                    break;
                case "clientcredentials":
                    break;
                case "federated":
                    break;
                default:
                    throw new UnauthorizedAccessException("Token type not supported.");
            }

            try
            {
                var validationParams = new TokenValidationParameters
                {
                    // Validate signature with secret key
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                    ValidateIssuerSigningKey = true,

                    // Issuer needs to be us
                    ValidIssuer = Issuer,
                    ValidateIssuer = true,

                    // Check that it is still valid.
                    ValidateLifetime = true,

                    ValidateAudience = false,

                    // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                    // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                    // machines which should have synchronised time, this can be set to zero. Where external tokens are
                    // used, some leeway here could be useful.
                    ClockSkew = TimeSpan.FromMinutes(0)
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                handler.ValidateToken(token.RawData, validationParams, out var _);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Token not valid", ex);
            }
        }
    }
}
