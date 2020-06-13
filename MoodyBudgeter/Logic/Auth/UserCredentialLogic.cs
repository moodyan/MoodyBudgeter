using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Repositories.Auth;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth
{
    public class UserCredentialLogic
    {
        private readonly ContextWrapper Context;

        public UserCredentialLogic(ContextWrapper context)
        {
            Context = context;
        }

        // This returns a user password! Do not return even the hashed and salted value from services!
        public async Task<UserCredential> GetUserCredential(string username)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserCredentialRepository(uow);

                return await repo.GetAll().Where(c => c.Username.ToUpper() == username.ToUpper()).FirstOrDefaultAsync();
            }
        }

        // This returns a user password! Do not return even the hashed and salted value from services!
        public async Task<UserCredential> GetUserCredential(int userId)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserCredentialRepository(uow);

                return await repo.GetAll().Where(c => c.UserId == userId).FirstOrDefaultAsync();
            }
        }

        public async Task<UserCredential> Create(UserCredential credential)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserCredentialRepository(uow);

                return await repo.Create(credential);
            }
        }

        public async Task<UserCredential> Update(UserCredential credential)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserCredentialRepository(uow);

                return await repo.Update(credential);
            }
        }
    }
}
