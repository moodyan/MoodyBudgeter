using MoodyBudgeter.Logic.Auth.Password;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Auth;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth
{
    public class UserLoginLogic
    {
        private readonly ContextWrapper Context;
        private readonly UserCredentialLogic UserCredentialLogic;

        public UserLoginLogic(ContextWrapper context)
        {
            Context = context;
            UserCredentialLogic = new UserCredentialLogic(context);
        }

        public async Task<UserCredential> GetUserLogin(int userId, bool includePortalZero = true)
        {
            var credential = await UserCredentialLogic.GetUserCredential(userId);

            if (credential != null)
            {
                credential.Clean();
            }

            return credential;
        }

        public async Task<UserCredential> CreateLogin(UserCredential userCredential)
        {
            if (userCredential.UserId <= 0)
            {
                throw new CallerException("UserId is required");
            }

            if (string.IsNullOrEmpty(userCredential.Password))
            {
                throw new CallerException("Password is required");
            }

            if (string.IsNullOrEmpty(userCredential.Username))
            {
                throw new CallerException("Username is required");
            }

            var userIdCredential = await UserCredentialLogic.GetUserCredential(userCredential.UserId);
            var usernameCredential = await UserCredentialLogic.GetUserCredential(userCredential.Username);

            if (userIdCredential != null || usernameCredential != null)
            {
                throw new CallerException("UserLogin already exists");
            }

            var passwordLogic = new PasswordLogic(Context);

            var passwordInvalid = await passwordLogic.ValidatePasswordRegex(userCredential.Password);

            if (passwordInvalid)
            {
                throw new FriendlyException("ValidateCredential.PasswordRegexInvalid", "Proposed password does not meet requirements.");
            }

            userCredential.Password = passwordLogic.HashAndSaltPassword(userCredential.Password, "", out string salt);
            userCredential.PasswordSalt = salt;

            var createdCredential = await UserCredentialLogic.Create(userCredential);

            createdCredential.Clean();

            return createdCredential;
        }

        public async Task<UserCredential> CreateEmptyLogin(int userId, string username)
        {
            if (userId <= 0)
            {
                throw new CallerException("UserId is required");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new CallerException("Username is required");
            }

            var userIdCredential = await UserCredentialLogic.GetUserCredential(userId);
            var usernameCredential = await UserCredentialLogic.GetUserCredential(username);

            if (userIdCredential != null || usernameCredential != null)
            {
                throw new CallerException("UserLogin already exists");
            }

            UserCredential userCredential = new UserCredential
            {
                UserId = userId,
                Username = username,
                Password = "",
                PasswordSalt = ""
            };

            var credential = await UserCredentialLogic.Create(userCredential);

            credential.Clean();

            return credential;
        }
    }
}
