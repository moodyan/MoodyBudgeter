using System;

namespace MoodyBudgeter.Data.Auth
{
    public class AuthUserSecurityRole
    {
        public int UserId { get; set; }
        public int SecurityRoleId { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }

        public AuthSecurityRole SecurityRole { get; set; }
        public AuthUserCredential UserCredential { get; set; }
    }
}
