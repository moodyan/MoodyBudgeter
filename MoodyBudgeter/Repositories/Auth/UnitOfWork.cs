using MoodyBudgeter.Data.Auth;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class UnitOfWork : IDisposable
    {
        public AuthContext DbContext { get; set; }

        private readonly AuthContextWrapper Context;

        public UnitOfWork(AuthContextWrapper context)
        {
            Context = context;

            if (context.Context == null)
            {
                DbContext = new AuthContext();
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
