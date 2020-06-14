using System;

namespace MoodyBudgeter.Data.Auth
{
    public class AuthUserLoginHistory
    {
        public int UserLoginHistoryId { get; set; }
        public int UserId { get; set; }
        public DateTime LoginDate { get; set; }
        public int TokenType { get; set; }
        public string Provider { get; set; }
        public string Audience { get; set; }
    }
}
