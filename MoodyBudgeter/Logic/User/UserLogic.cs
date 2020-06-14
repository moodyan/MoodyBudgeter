using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UserLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public UserLogic(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<BudgetUser> GetUserWithRelated(int userId, bool isAdmin)
        {
            if (userId <= 0)
            {
                throw new CallerException("This user is anonymous.");
            }

            var user = await GetUser(userId);

            user.UserProfileProperties = await new UserProfilePropertyLogic(Cache, Context).GetUserProfileProperties(userId, isAdmin);

            user.UserRoles = await new UserRoleLogic(Cache, Context).GetUserRoles(userId, isAdmin);

            return user;
        }

        public async Task<BudgetUser> GetUserWithoutRelated(int userId)
        {
            UserCacheLogic userCache = new UserCacheLogic(Cache);
            BudgetUser user = await userCache.GetUserFromCache(userId);

            if (user != null)
            {
                return user;
            }

            var dbUser = await GetDBUser(userId);

            user = new BudgetUser()
            {
                UserId = dbUser.UserId,
                Username = dbUser.Username
            };

            await userCache.AddUserToCache(user);

            return user;

        }
        
        private async Task<BudgetUser> GetUser(int userId)
        {
            var userCache = new UserCacheLogic(Cache);

            BudgetUser user = await userCache.GetUserFromCache(userId);

            if (user != null)
            {
                return user;
            }

            user = await GetDBUser(userId);

            await userCache.AddUserToCache(user);

            return user;
        }
        
        public async Task<BudgetUser> GetDBUser(int userId)
        {
            BudgetUser user;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);

                user = await repo.Find(userId);
            }

            return user;
        }

        public async Task<BudgetUser> GetDBUser(string username)
        {
            BudgetUser user;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);

                user = await repo.GetAll().Where(u => u.Username == username).FirstOrDefaultAsync();
            }

            return user;
        }

        public async Task<BudgetUser> CreateUser(BudgetUser user)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);

                return await repo.Create(user);
            }
        }

        public async Task UpdateDBUser(BudgetUser user)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);

                await repo.Update(user);
            }
        }
    }
}
