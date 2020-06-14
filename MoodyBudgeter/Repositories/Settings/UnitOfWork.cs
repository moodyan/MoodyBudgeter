using MoodyBudgeter.Data.Settings;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Settings
{
    public class UnitOfWork : IDisposable
    {
        public SettingContext DbContext { get; set; }

        private readonly SettingsContextWrapper Context;

        public UnitOfWork(SettingsContextWrapper context)
        {
            Context = context;

            if (context.Context == null)
            {
                DbContext = new SettingContext();
            }
            else
            {
                DbContext = context.Context;
            }
        }

        public async Task SaveChanges()
        {
            await DbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (Context.Injected)
            {
                return;
            }

            DbContext.Dispose();
        }
    }
}
