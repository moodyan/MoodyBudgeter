using MoodyBudgeter.Data.User;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.User
{
    public class UnitOfWork : IDisposable
    {
        public UserContext DbContext { get; set; }

        private readonly UserContextWrapper Context;

        public UnitOfWork(UserContextWrapper context)
        {
            Context = context;
            if (context.Context == null)
            {
                DbContext = new UserContext();
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