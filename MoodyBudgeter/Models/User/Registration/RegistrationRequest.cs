using System.Collections.Generic;

namespace MoodyBudgeter.Models.User.Registration
{
    public class RegistrationRequest
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ReferredBy { get; set; }
        public string RegistrationChannel { get; set; }
        public bool CreateRegistrationToken { get; set; }
        public string CodeChallenge { get; set; }
        public string RegistrationToken { get; set; }
        public bool? SendRegistrationEmail { get; set; }
        public List<UserProfileProperty> UserProfileProperties { get; set; }
    }
}
