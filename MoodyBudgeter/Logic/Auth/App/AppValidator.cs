using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.App;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.App
{
    public class AppValidator
    {
        private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public AppValidator(IBudgeterCache cache, ContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task ValidateImplicit(string clientId, string redirectUri)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(clientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (!app.AllowImplicit)
            {
                throw new CallerException("Invalid grant");
            }

            if (app.RedirectUri != redirectUri)
            {
                throw new CallerException("Invalid redirect_uri");
            }
        }

        public async Task ValidateAuthCode(string clientId, string redirectUri, string code_challenge_method, string code_challenge)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(clientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (!app.AllowAuthCode)
            {
                throw new CallerException("Invalid grant");
            }

            if (app.RedirectUri != redirectUri)
            {
                throw new CallerException("Invalid redirect_uri");
            }

            if (code_challenge_method != "S256")
            {
                throw new CallerException("Challenge method not supported");
            }

            if (string.IsNullOrEmpty(code_challenge))
            {
                throw new CallerException("Challenge is required");
            }

            if (string.IsNullOrEmpty(app.ClientSecret))
            {
                throw new CallerException("No client secret on app.");
            }
        }

        public async Task ValidateLogout(string clientId, string redirectUri)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(clientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (app.RedirectUri != redirectUri)
            {
                throw new CallerException("Invalid redirect_uri");
            }
        }

        public async Task ValidateAuthCodeTokenRequest(TokenRequest tokenRequest, string authHeader)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(tokenRequest.ClientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (!app.AllowAuthCode)
            {
                throw new CallerException("Invalid grant");
            }

            if (app.RedirectUri != tokenRequest.RedirectUri)
            {
                throw new CallerException("Invalid redirect_uri");
            }

            ValidateSecret(tokenRequest, authHeader, app);
        }

        public async Task ValidateRefreshTokenRequest(TokenRequest tokenRequest, string authHeader)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(tokenRequest.ClientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (!app.AllowAuthCode)
            {
                throw new CallerException("Invalid grant");
            }

            ValidateSecret(tokenRequest, authHeader, app);
        }

        public async Task<AppModel> ValidateClientCredentialsTokenRequest(TokenRequest tokenRequest, string authHeader)
        {
            var appLogic = new AppLogic(Cache, Context);

            var app = await appLogic.GetApp(tokenRequest.ClientId);

            if (app == null)
            {
                throw new CallerException("Invalid client");
            }

            if (!app.AllowClientCredentials)
            {
                throw new CallerException("Invalid grant");
            }

            ValidateSecret(tokenRequest, authHeader, app);

            if (app.UserId == null || app.UserId <= 0)
            {
                throw new CallerException("Invalid app userId");
            }

            return app;
        }

        private void ValidateSecret(TokenRequest tokenRequest, string authHeader, AppModel app)
        {
            if (string.IsNullOrEmpty(app.ClientSecret))
            {
                throw new CallerException("No client secret on app.");
            }

            if (string.IsNullOrEmpty(authHeader))
            {
                throw new CallerException("No auth header.");
            }

            if (!authHeader.StartsWith("Basic "))
            {
                throw new CallerException("Unrecognized auth header");
            }

            authHeader = authHeader.Replace("Basic ", "");

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword = encoding.GetString(Convert.FromBase64String(authHeader));

            string clientId = usernamePassword.Split(":")[0];
            string clientSecret = usernamePassword.Split(":")[1];

            if (clientId != tokenRequest.ClientId)
            {
                throw new CallerException("Invalid client");
            }

            if (clientSecret != app.ClientSecret)
            {
                throw new CallerException("Invalid client secret");
            }
        }
    }
}
