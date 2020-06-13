using System.Collections.Generic;

namespace MoodyBudgeter.Data.Auth
{
    public class AuthSecurityRole
    {
        public AuthSecurityRole()
        {
            UserSecurityRole = new HashSet<AuthUserSecurityRole>();
        }

        public int SecurityRoleId { get; set; }
        public string Name { get; set; }

        public ICollection<AuthUserSecurityRole> UserSecurityRole { get; set; }
    }
}
