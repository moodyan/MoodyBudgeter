using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Password;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Registration;
using MoodyBudgeter.Repositories.Auth;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Password
{
    public class PasswordResetLogic
    {
        private readonly ContextWrapper Context;
        private readonly ContextWrapper UserContext;

        private const int RESET_TOKEN_BYTE_LENGTH = 20;
        private const int RESET_TIME_IN_MINUTES = 60;

        public PasswordResetLogic(ContextWrapper context)
        {
            Context = context;
        }

        public async Task ResetPassword(string username)
        {
            var userCredentialLogic = new UserCredentialLogic(Context);

            var credential = await userCredentialLogic.GetUserCredential(username);

            if (credential == null)
            {
                credential = await FindAndCreateCredentialFromResetText(username);
            }

            await ResetPassword(userCredentialLogic, credential);
        }

        public async Task<UserCredential> FindAndCreateCredentialFromResetText(string resetEntry)
        {
            var userLoginLogic = new UserLoginLogic(Context);
            var userLogic = new UserLogic(Context);

            // Search by username
            var result = await userLogic.FindUserByUsername(resetEntry);

            if (result != null && result.Records.Count > 0)
            {
                var userResult = result.Records.FirstOrDefault();

                return await userLoginLogic.CreateEmptyLogin(userResult.UserId, userResult.SearchFieldValue);
            }

            // Search by email
            var emailResult = await userLogic.FindUserByEmail(resetEntry);

            if (emailResult != null && emailResult.Records.Count > 0)
            {
                if (emailResult.TotalRecordCount > 1)
                {
                    // Should this be friendly? What can we even do if this happens?
                    // We could only check email if it marked unique (this should mean the portal is using a users real email)
                    throw new CallerException("Multiple users have this email");
                }

                var emailUserResult = emailResult.Records.FirstOrDefault();

                var user = await userLogic.GetUser(emailUserResult.UserId);

                var userCredentialLogic = new UserCredentialLogic(Context);

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
            var userCredentialLogic = new UserCredentialLogic(Context);
            var userLogic = new UserLogic(Context);

            var credential = await userCredentialLogic.GetUserCredential(userId);

            if (credential == null)
            {
                var user = await userLogic.GetUser(userId);

                var userLoginLogic = new UserLoginLogic(Context);

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

            var message = new PasswordReset
            {
                UserId = userCredential.UserId,
                ResetToken = userCredential.ResetToken
            };

            await QueueSender.SendMessage<PasswordReset>(message);
        }

        public async Task<string> CreateEmptyCredentialsWithResetToken(int userId, string username)
        {
            var userLoginLogic = new UserLoginLogic(Context);

            var userCredential = await userLoginLogic.CreateEmptyLogin(userId, username);

            userCredential.ResetToken = GenerateResetToken();
            userCredential.ResetExpiration = DateTime.UtcNow.AddMinutes(RESET_TIME_IN_MINUTES);

            var userCredentialLogic = new UserCredentialLogic(Context);

            await userCredentialLogic.Update(userCredential);

            return userCredential.ResetToken;
        }

        public async Task<int> ProcessReset(string resetToken, string newPassword)
        {
            var credential = await ValidatePasswordResetToken(resetToken);
            var userLogic = new UserLogic(Context);

            await new PasswordLogic(Context).ChangePassword(credential, newPassword);

            if (await ShouldEnableUser())
            {
                await userLogic.EnableUser(credential.UserId);
            }

            return credential.UserId;
        }

        private async Task<bool> ShouldEnableUser()
        {
            string setting = await SettingsRequester.GetSetting("SystemSettings_UserRegistration");

            if (!int.TryParse(setting, out int type))
            {
                return false;
            }

            var registrationType = (RegistrationType)type;

            if (registrationType == RegistrationType.Verified)
            {
                return true;
            }

            return false;
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

            using (var uow = new UnitOfWork(Context))
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
