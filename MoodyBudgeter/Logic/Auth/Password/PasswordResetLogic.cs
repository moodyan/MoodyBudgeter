using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Logic.User.Search;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Password
{
    public class PasswordResetLogic
    {
        private readonly AuthContextWrapper AuthContext;
        private readonly UserContextWrapper UserContext;
        private readonly IBudgeterCache Cache;

        private const int RESET_TOKEN_BYTE_LENGTH = 20;
        private const int RESET_TIME_IN_MINUTES = 60;

        public PasswordResetLogic(IBudgeterCache cache, AuthContextWrapper authContext, UserContextWrapper userContext)
        {
            AuthContext = authContext;
            UserContext = userContext;
            Cache = cache;
        }

        public async Task ResetPassword(string username)
        {
            var userCredentialLogic = new UserCredentialLogic(AuthContext);

            var credential = await userCredentialLogic.GetUserCredential(username);

            if (credential == null)
            {
                credential = await FindAndCreateCredentialFromResetText(username);
            }

            await ResetPassword(userCredentialLogic, credential);
        }

        public async Task<UserCredential> FindAndCreateCredentialFromResetText(string resetEntry)
        {
            var userLoginLogic = new UserLoginLogic(AuthContext);
            var searchLogic = new SearchLogic(Cache, UserContext);
            var userLogic = new UserLogic(Cache, UserContext);

            UserSearch usernameSearch = new UserSearch
            {
                SearchText = resetEntry,
                SearchUsername = true,
                Operator = SearchOperator.Equals,
                PageSize = 1
            };

            // Search by username
            Page<UserSearchResponse> result = await searchLogic.Search(usernameSearch);

            if (result != null && result.Records.Count > 0)
            {
                var userResult = result.Records.FirstOrDefault();

                return await userLoginLogic.CreateEmptyLogin(userResult.UserId, userResult.SearchFieldValue);
            }

            UserSearch emailSearch = new UserSearch
            {
                SearchText = resetEntry,
                ProfilePropertyName = "email",
                Operator = SearchOperator.Equals,
                PageSize = 1
            };

            // Search by email
            Page<UserSearchResponse> emailResult = await searchLogic.Search(emailSearch);

            if (emailResult != null && emailResult.Records.Count > 0)
            {
                if (emailResult.TotalRecordCount > 1)
                {
                    // Should this be friendly? What can we even do if this happens?
                    // We could only check email if it marked unique
                    throw new CallerException("Multiple users have this email");
                }

                var emailUserResult = emailResult.Records.FirstOrDefault();

                var user = await userLogic.GetUserWithoutRelated(emailUserResult.UserId);

                var userCredentialLogic = new UserCredentialLogic(AuthContext);

                var credential = await userCredentialLogic.GetUserCredential(user.Username);

                if (credential != null)
                {
                    return credential;
                }

                return await userLoginLogic.CreateEmptyLogin(user.UserId, user.Username);
            }

            return null;
        }

        public async Task ResetPassword(int userId)
        {
            var userCredentialLogic = new UserCredentialLogic(AuthContext);
            var userLogic = new UserLogic(Cache, UserContext);

            var credential = await userCredentialLogic.GetUserCredential(userId);

            if (credential == null)
            {
                var user = await userLogic.GetUserWithoutRelated(userId);

                var userLoginLogic = new UserLoginLogic(AuthContext);

                credential = await userLoginLogic.CreateEmptyLogin(userId, user.Username);
            }

            await ResetPassword(userCredentialLogic, credential);
        }

        private async Task ResetPassword(UserCredentialLogic userCredentialLogic, UserCredential userCredential)
        {
            if (userCredential == null)
            {
                throw new FriendlyException("PasswordReset.UserNotFound", "User does not exist");
            }

            userCredential.ResetToken = GenerateResetToken();
            userCredential.ResetExpiration = DateTime.UtcNow.AddMinutes(RESET_TIME_IN_MINUTES);

            await userCredentialLogic.Update(userCredential);

            //var message = new PasswordReset
            //{
            //    UserId = userCredential.UserId,
            //    ResetToken = userCredential.ResetToken
            //};

            //await QueueSender.SendMessage<PasswordReset>(message);
        }

        public async Task<string> CreateEmptyCredentialsWithResetToken(int userId, string username)
        {
            var userLoginLogic = new UserLoginLogic(AuthContext);

            var userCredential = await userLoginLogic.CreateEmptyLogin(userId, username);

            userCredential.ResetToken = GenerateResetToken();
            userCredential.ResetExpiration = DateTime.UtcNow.AddMinutes(RESET_TIME_IN_MINUTES);

            var userCredentialLogic = new UserCredentialLogic(AuthContext);

            await userCredentialLogic.Update(userCredential);

            return userCredential.ResetToken;
        }

        public async Task<int> ProcessReset(string resetToken, string newPassword)
        {
            var credential = await ValidatePasswordResetToken(resetToken);

            await new PasswordLogic(AuthContext).ChangePassword(credential, newPassword);

            return credential.UserId;
        }

        private string GenerateResetToken()
        {
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[RESET_TOKEN_BYTE_LENGTH];
            cryptoRandomDataGenerator.GetBytes(buffer);

            return BitConverter.ToString(buffer).ToLower().Replace("-", "");
        }

        public async Task<UserCredential> ValidatePasswordResetToken(string resetToken)
        {
            if (string.IsNullOrEmpty(resetToken))
            {
                throw new CallerException("ResetToken missing");
            }

            UserCredential userCredential;

            using (var uow = new Repositories.Auth.UnitOfWork(AuthContext))
            {
                var repo = new UserCredentialRepository(uow);

                // Index needed?
                userCredential = await repo.GetAll().Where(c => c.ResetToken == resetToken && c.ResetExpiration != null && c.ResetExpiration > DateTime.UtcNow).FirstOrDefaultAsync();
            }

            if (userCredential == null)
            {
                throw new FriendlyException("PasswordReset.TokenInvalid", "Reset token is not valid");
            }

            return userCredential;
        }
    }
}
