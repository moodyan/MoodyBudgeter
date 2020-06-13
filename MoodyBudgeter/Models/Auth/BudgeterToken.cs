namespace MoodyBudgeter.Models.Auth
{
    public class BudgeterToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
