using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.Auth.Password
{
    public class PasswordReset
    {
        public int UserId { get; set; }
        public string ResetToken { get; set; }
    }
}
