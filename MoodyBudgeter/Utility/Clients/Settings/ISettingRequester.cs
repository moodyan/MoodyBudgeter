using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.Settings
{
    public interface ISettingRequester
    {
        Task<string> GetSetting(string settingName);
    }
}
