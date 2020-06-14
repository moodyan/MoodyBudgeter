using MoodyBudgeter.Logic.Settings;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Settings;
using MoodyBudgeter.Utility.Cache;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.Settings
{
    public class SettingRequester : ISettingRequester
    {
        private readonly IBudgeterCache Cache;
        private readonly SettingsContextWrapper Context;

        public SettingRequester(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new SettingsContextWrapper();
        }
        public async Task<string> GetSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new CallerException("SettingName Cannot Be Blank");
            }

            string settingValue = await Cache.Get<string>(GetCacheKey(settingName));

            if (settingValue != null)
            {
                return settingValue;
            }

            var settingLogic = new SettingsLogic(Cache, Context);

            settingValue = await settingLogic.GetStrSetting(settingName);

            return settingValue;
        }

        private string GetCacheKey(string settingName)
        {
            return "SiteSettings:" + settingName.ToLower();
        }
    }
}
