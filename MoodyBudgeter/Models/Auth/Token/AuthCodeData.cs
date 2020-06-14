namespace MoodyBudgeter.Models.Auth
{
    public class AuthCodeData
    {
        public string ClientId { get; set; }
        public string CodeChallenge { get; set; }
        public int UserId { get; set; }
    }
}
