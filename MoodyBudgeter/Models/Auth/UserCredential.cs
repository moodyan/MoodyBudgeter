using System;

namespace MoodyBudgeter.Models.Auth
{
    public class UserCredential
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public int? AttemptCount { get; set; }
        public DateTime? FirstAttemptDate { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetExpiration { get; set; }
        public DateTime DateCreated { get; set; }

        public void Clean()
        {
            Password = "";
            PasswordSalt = "";
            ResetToken = "";
            ResetExpiration = null;
        }
    }
}
