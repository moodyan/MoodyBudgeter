using MoodyBudgeter.Data.Settings;

namespace MoodyBudgeter.Repositories.Settings
{
    public class ContextWrapper
    {
        public SettingContext Context { get; set; }
        public bool Injected { get; set; }

        public ContextWrapper() { }

        public ContextWrapper(SettingContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
