using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Settings;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Settings;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.Settings
{
    [Route("setting/v1/[controller]")]
    public class SettingController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly SettingsContextWrapper Context;

        public SettingController(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new SettingsContextWrapper();
        }

        [Route("{settingname}"), HttpGet]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<ActionResult> Get(string settingname)
        {
            var settingLogic = new SettingsLogic(Cache, Context);

            return Json(await settingLogic.GetStrSetting(settingname));
        }

        [Route("{settingname}"), HttpPut]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<SiteSetting> Put(string settingname, [FromBody]SiteSetting setting)
        {
            setting.SettingName = settingname;

            var settingLogic = new SettingsLogic(Cache, Context);

            return await settingLogic.CreateOrUpdateSetting(setting);
        }
    }
}
