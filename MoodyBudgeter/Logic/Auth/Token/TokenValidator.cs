using Microsoft.IdentityModel.Tokens;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class TokenValidator
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        
        private readonly string SecretKey;
        private readonly string Issuer;

        public TokenValidator(IEnvironmentRequester environmentRequester)
        {
            SecretKey = environmentRequester.GetVariable("TokenSecretKey");
            Issuer = environmentRequester.GetVariable("Issuer");
        }

        public void ValidateToken(string strToken, TokenType type)
        {
            JwtSecurityToken token;

            try
            {
                token = new JwtSecurityToken(strToken);
            }
            catch (Exception)
            {
                throw new CallerException("Unable to decode token");
            }

            Validate(token);

            ValidateTokenClaims(token.Claims, type);
        }

        private void Validate(JwtSecurityToken token)
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

            try
            {
                handler.ValidateToken(token.RawData, validationParams, out var _);
            }
            catch (Exception)
            {
                throw new CallerException("Token is not valid");
            }
        }

        private void ValidateTokenClaims(IEnumerable<Claim> claims, TokenType type)
        {
            foreach (var claim in claims)
            {
                if (claim.Type == "uid")
                {
                    UserId = int.Parse(claim.Value);
                }

                if (claim.Type == "typ")
                {
                    if (claim.Value != type.ToString().ToLower())
                    {
                        throw new CallerException("Token is not the correct type.");
                    }
                }

                if (claim.Type == "usr")
                {
                    Username = claim.Value;
                }
            }
        }
    }
}
