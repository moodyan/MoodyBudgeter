using MoodyBudgeter.Models.Auth.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.Auth
{
    public class UserLoginHistory
    {
        public int UserId { get; set; }
        public DateTime LoginDate { get; set; }
        public TokenType TokenType { get; set; }
        public string Provider { get; set; }
        public string Audience { get; set; }
    }
}
