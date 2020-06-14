using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Repositories.Auth;
using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Password
{
    public class PasswordLogic
    {
        private readonly UserCredentialLogic UserCredentialLogic;

        private const int PASSWORD_ATTEMPT_MAX_COUNT = 10;
        private const int PASSWORD_ATTEMPT_MINUTE_WINDOW = 5;

        public PasswordLogic(AuthContextWrapper context)
        {
            UserCredentialLogic = new UserCredentialLogic(context);
        }

        public async Task<UserCredential> ValidateUserCredentials(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new CallerException("No Username");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new CallerException("No Password");
            }

            var credential = await UserCredentialLogic.GetUserCredential(username);

            await Validate(credential, password);

            credential.Clean();

            return credential;
        }

        private async Task Validate(UserCredential credential, string password)
        {
            if (credential == null)
            {
                throw new FriendlyException("ValidateCredential.UserNotFound", "User not found");
            }

            if (string.IsNullOrEmpty(credential.Password))
            {
                throw new FriendlyException("ValidateCredential.IncorrectPassword", "Incorrect Password");
            }

            if (credential.AttemptCount >= PASSWORD_ATTEMPT_MAX_COUNT && credential.FirstAttemptDate >= DateTime.UtcNow.AddMinutes(-PASSWORD_ATTEMPT_MINUTE_WINDOW))
            {
                throw new FriendlyException("ValidateCredential.PasswordAttemptsExceeded", "Too many password attempts, please wait before trying again");
            }

            string hashedPassword = HashAndSaltPassword(password, credential.PasswordSalt, out string salt);

            if (credential.Password != hashedPassword)
            {
                // Record invalid try.
                if (credential.FirstAttemptDate == null)
                {
                    credential.FirstAttemptDate = DateTime.UtcNow;
                    credential.AttemptCount = 1;
                }
                else
                {
                    if (credential.FirstAttemptDate < DateTime.UtcNow.AddMinutes(-PASSWORD_ATTEMPT_MINUTE_WINDOW))
                    {
                        credential.AttemptCount = 1;
                        credential.FirstAttemptDate = DateTime.UtcNow;
                    }
                    else
                    {
                        credential.AttemptCount++;
                    }
                }

                credential = await UserCredentialLogic.Update(credential);

                throw new FriendlyException("ValidateCredential.IncorrectPassword", "Incorrect Password");
            }
        }

        public async Task ChangePassword(int userId, string previousPassword, string proposedPassword)
        {
            var credential = await UserCredentialLogic.GetUserCredential(userId);

            await Validate(credential, previousPassword);

            await ChangePassword(credential, proposedPassword);
        }

        public async Task ChangePassword(UserCredential credential, string proposedPassword)
        {
            if (credential == null)
            {
                throw new CallerException("No login entry for user");
            }

            if (string.IsNullOrEmpty(proposedPassword))
            {
                throw new CallerException("New password required");
            }
            
            if (ValidatePasswordRegex(proposedPassword))
            {
                throw new FriendlyException("ValidateCredential.PasswordRegexInvalid", "Proposed password does not meet requirements.");
            }
            
            credential.Password = HashAndSaltPassword(proposedPassword, "", out string salt);
            credential.PasswordSalt = salt;
            credential.ResetExpiration = null;
            credential.ResetToken = null;

            await UserCredentialLogic.Update(credential);
        }

        public string HashAndSaltPassword(string password, string existingSalt, out string salt)
        {
            byte[] byteSalt = new byte[32];

            if (string.IsNullOrEmpty(existingSalt))
            {
                new RNGCryptoServiceProvider().GetBytes(byteSalt);
            }
            else
            {
                byteSalt = Convert.FromBase64String(existingSalt);
            }

            // Default iteration is 1000 hashes
            var deriveBytes = new Rfc2898DeriveBytes(password, byteSalt);

            byte[] hash = deriveBytes.GetBytes(32);
            byte[] deriveSalt = deriveBytes.Salt;

            salt = Convert.ToBase64String(deriveSalt);

            return Convert.ToBase64String(hash);
        }

        public bool ValidatePasswordRegex(string password)
        {
            string passwordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$";

            return !Regex.IsMatch(password, passwordRegex);
        }

        public async Task ResetAttemptCount(int userId)
        {
            var userCredential = await UserCredentialLogic.GetUserCredential(userId);

            userCredential.AttemptCount = 0;

            await UserCredentialLogic.Update(userCredential);
        }
    }
}
