using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Data.Auth
{
    public class AuthUserCredential
    {
        public AuthUserCredential()
        {
            UserSecurityRole = new HashSet<AuthUserSecurityRole>();
        }

        public int PortalId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public int? AttemptCount { get; set; }
        public DateTime? FirstAttemptDate { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetExpiration { get; set; }
        public DateTime DateCreated { get; set; }

        public ICollection<AuthUserSecurityRole> UserSecurityRole { get; set; }
    }
}
}
