using MoodyBudgeter.Data.Auth;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class UnitOfWork : IDisposable
    {
        public AuthContext DbContext { get; set; }

        private readonly ContextWrapper Context;

        public UnitOfWork(ContextWrapper context)
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
