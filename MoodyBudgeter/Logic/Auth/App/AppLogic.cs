using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Auth.App;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.App
{
    public class AppLogic
    {
        private readonly AppCache AppCache;
        private readonly AuthContextWrapper Context;

        public AppLogic(IBudgeterCache cache, AuthContextWrapper context)
        {
            AppCache = new AppCache(cache);
            Context = context;
        }

        public async Task<AppModel> GetApp(string clientId)
        {
            var cacheResult = await AppCache.GetAppFromCache(clientId);

            if (cacheResult != null)
            {
                return cacheResult;
            }

            AppModel app;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new AppRepository(uow);

                app = await repo.GetAll().Where(c => c.ClientId == clientId).FirstOrDefaultAsync();
            }

            if (app != null)
            {
                await AppCache.AddAppToCache(clientId, app);
            }

            return app;
        }

        public async Task<List<AppModel>> GetApps()
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new AppRepository(uow);

                return await repo.GetAll().ToListAsync();
            }
        }

        public async Task<AppModel> Create(AppModel app, bool createSecret)
        {
            if (string.IsNullOrEmpty(app.UpdateBy))
            {
                throw new CallerException("UpdateBy is required");
            }

            app.ClientId = GenerateKey(10);

            if (createSecret)
            {
                app.ClientSecret = GenerateKey(40);
            }
            else
            {
                app.ClientSecret = "";
            }

            Validate(app, createSecret);

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new AppRepository(uow);

                return await repo.Create(app);
            }
        }

        private string GenerateKey(int length)
        {
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[length];
            cryptoRandomDataGenerator.GetBytes(buffer);

            return BitConverter.ToString(buffer).ToLower().Replace("-", "");
        }

        public async Task<AppModel> Update(string clientId, AppModel app)
        {
            if (string.IsNullOrEmpty(app.UpdateBy))
            {
                throw new CallerException("UpdateBy is required");
            }

            var appToUpdate = await GetApp(clientId);

            if (appToUpdate == null)
            {
                throw new CallerException("App not found");
            }

            app.ClientId = clientId;

            Validate(app, !string.IsNullOrEmpty(appToUpdate.ClientSecret));

            AppModel updatedApp;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new AppRepository(uow);

                updatedApp = await repo.Update(app);
            }

            await AppCache.InvalidateAppCache(updatedApp.ClientId);

            return updatedApp;
        }

        public async Task Delete(string clientId)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new AppRepository(uow);

                await repo.Delete(clientId);
            }

            await AppCache.InvalidateAppCache(clientId);
        }

        private void Validate(AppModel app, bool appHasSecret)
        {
            if (string.IsNullOrEmpty(app.Name))
            {
                throw new CallerException("App Name is required");
            }

            if (app.AllowImplicit && string.IsNullOrEmpty(app.RedirectUri))
            {
                throw new CallerException("RedirectUri is required for implicit auth");
            }

            if (app.AllowAuthCode)
            {
                if (!appHasSecret)
                {
                    throw new CallerException("Client Secret is required for auth code login");
                }

                if (string.IsNullOrEmpty(app.RedirectUri))
                {
                    throw new CallerException("RedirectUri is required for auth code flow");
                }
            }

            if (app.AllowClientCredentials)
            {
                if (!appHasSecret)
                {
                    throw new CallerException("Client Secret is required for client credentials");
                }

                if (app.UserId == null)
                {
                    throw new CallerException("UserId is required for a ClientCredentials grant type");
                }
            }
        }
    }
}
