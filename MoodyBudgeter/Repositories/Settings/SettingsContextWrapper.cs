using MoodyBudgeter.Data.Settings;

namespace MoodyBudgeter.Repositories.Settings
{
    public class SettingsContextWrapper
    {
        public SettingContext Context { get; set; }
        public bool Injected { get; set; }

        public SettingsContextWrapper() { }

        public SettingsContextWrapper(SettingContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
