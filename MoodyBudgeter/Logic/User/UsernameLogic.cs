using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UsernameLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public UsernameLogic(IBudgeterCache cache, ContextWrapper context)
        {
            Context = context;
            Cache = cache;
        }

        public async Task<BudgetUser> UpdateUsername(int userId, string proposedUsername)
        {
            await ChangeUsername(userId, proposedUsername);

            UserLogic loyaltyUserLogic = new UserLogic(Cache, Context);

            return await loyaltyUserLogic.GetDBUser(userId);
        }

        public async Task ChangeUsername(int userId, string proposedUsername)
        {
            if (userId == 1)
            {
                throw new CallerException("Host username cannot be changed.");
            }

            UserLogic loyaltyUserLogic = new UserLogic(Cache, Context);

            BudgetUser originalUser = await loyaltyUserLogic.GetUserWithoutRelated(userId);

            if (originalUser == null)
            {
                throw new CallerException("Cannot find user.");
            }

            await ValidateUsername(proposedUsername);
            await UpdateUserRecordUsername(userId, proposedUsername);

            //var userCacheLogic = new UserCacheLogic(Cache);
            //await userCacheLogic.InvalidateLoyaltyUserCache(userId);
        }

        public async Task ValidateUsername(string proposedUsername)
        {
            if (proposedUsername.Any(char.IsWhiteSpace))
            {
                throw new CallerException("Username may not contain spaces.");
            }

            bool usernameInvalid = await ValidateUsernameRegex(proposedUsername);

            if (usernameInvalid)
            {
                throw new FriendlyException("Username.InvalidUsername", "Proposed username does not meet requirements.");
            }

            int userCount;

            using (UnitOfWork uow = new UnitOfWork(Context))
            {
                UserRepository repo = new UserRepository(uow);

                userCount = await repo.GetAll().Where(c => c.Username.ToUpper() == proposedUsername.ToUpper()).CountAsync();
            }

            if (userCount >= 1)
            {
                throw new FriendlyException("Username.InUse", "Username is already in use.");
            }
        }

        public async Task UpdateUserRecordUsername(int userId, string newUsername)
        {
            UserLogic userLogic = new UserLogic(Cache, Context);

            BudgetUser user = await userLogic.GetDBUser(userId);

            user.Username = newUsername;

            await userLogic.UpdateDBUser(user);
        }

        public async Task<bool> ValidateUsernameRegex(string username)
        {
            bool validUsername = false;

            string usernameRegex = await SettingRequester.GetSetting("UsernameValidationExpression");

            if (!string.IsNullOrEmpty(usernameRegex))
            {
                validUsername = Regex.IsMatch(username, usernameRegex);
            }

            return validUsername;
        }
    }
}