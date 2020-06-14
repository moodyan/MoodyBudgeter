using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class AuthCodeLogic
    {
        private readonly IBudgeterCache Cache;

        private const int CODE_VALID_TIME = 300;

        public AuthCodeLogic(IBudgeterCache cache)
        {
            Cache = cache;
        }

        public async Task<string> CreateCode(int userId, string client_id, string code_challenge)
        {
            string code = Guid.NewGuid().ToString();

            var codeData = new AuthCodeData
            {
                ClientId = client_id,
                CodeChallenge = code_challenge,
                UserId = userId
            };

            await Cache.Insert(GetCacheKey(code), codeData, new TimeSpan(0, 0, CODE_VALID_TIME));

            return code;
        }

        public async Task<int> ValidateCode(TokenRequest tokenRequest)
        {
            var codeData = await Cache.Get<AuthCodeData>(GetCacheKey(tokenRequest.Code));

            if (codeData == null)
            {
                throw new CallerException("Invalid code");
            }

            await Cache.Remove(GetCacheKey(tokenRequest.Code));

            if (codeData.ClientId != tokenRequest.ClientId)
            {
                throw new CallerException("Invalid client");
            }

            if (string.IsNullOrEmpty(codeData.CodeChallenge) || string.IsNullOrEmpty(tokenRequest.CodeVerifier))
            {
                throw new CallerException("Missing PKCE info");
            }

            VerifyCode(codeData.CodeChallenge, tokenRequest.CodeVerifier);

            return codeData.UserId;
        }

        private void VerifyCode(string codeChallenge, string codeVerifier)
        {
            var sha = new SHA256Managed();
            sha.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

            string verifierHash = Convert.ToBase64String(sha.Hash).Replace('+', '-').Replace('/', '_').Replace("=", "");

            if (verifierHash != codeChallenge)
            {
                throw new CallerException("Invalid verifier");
            }
        }

        private string GetCacheKey(string code)
        {
            return "Budgeter:Auth:AuthCode:Code_" + code;
        }
    }
}
