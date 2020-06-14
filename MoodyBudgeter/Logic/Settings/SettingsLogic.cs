using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Repositories.Settings;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Settings
{
    public class SettingsLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public SettingsLogic(IBudgeterCache cache, ContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<string> GetStrSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new CallerException("Valid Setting Name Required");
            }

            string settingValue = await Cache.Get<string>(GetCacheKey(settingName));

            if (settingValue != null)
            {
                return settingValue;
            }

            var setting = await GetSetting(settingName);

            if (setting == null || setting.SettingValue == null)
            {
                settingValue = "";
            }
            else
            {
                settingValue = setting.SettingValue;
            }

            await Cache.Insert(GetCacheKey(settingName), settingValue, new TimeSpan(6, 0, 0));

            return settingValue;
        }

        public async Task<SiteSetting> GetSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new CallerException("Valid Setting Name Required");
            }

            SiteSetting setting;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteSettingRepository(uow);

                setting = await repo.GetAll().Where(x => x.SettingName.ToUpper() == settingName.ToUpper()).FirstOrDefaultAsync();
            }

            return setting;
        }

        public async Task<SiteSetting> CreateOrUpdateSetting(SiteSetting setting)
        {
            var existingSetting = await GetSetting(setting.SettingName);

            if (existingSetting == null)
            {
                setting = await CreateSetting(setting);
            }
            else
            {
                setting.SiteSettingId = existingSetting.SiteSettingId;

                setting = await UpdateSetting(setting);
            }

            await Cache.Remove(GetCacheKey(setting.SettingName));

            return setting;
        }

        public async Task<SiteSetting> CreateSetting(SiteSetting setting)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteSettingRepository(uow);

                return await repo.Create(setting);
            }
        }

        public async Task<SiteSetting> UpdateSetting(SiteSetting setting)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteSettingRepository(uow);

                return await repo.Update(setting);
            }
        }

        private string GetCacheKey(string settingName)
        {
            return "SystemSettings:" + settingName.ToLower();
        }
    }
}
