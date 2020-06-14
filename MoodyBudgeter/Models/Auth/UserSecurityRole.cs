using MoodyBudgeter.Models.User.Roles;
using System;

namespace MoodyBudgeter.Models.Auth
{
    public class UserSecurityRole
    {
        public int UserId { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
    }
}
