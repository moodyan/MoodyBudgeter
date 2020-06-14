namespace MoodyBudgeter.Models.Auth.Google
{
    public class GoogleSSOResponse
    {
        public GoogleSSOResponse(GoogleTokenResponse googleToken, BudgeterToken budgeterToken)
        {
            GoogleToken = googleToken;
            BudgeterToken = budgeterToken;
        }

        public GoogleTokenResponse GoogleToken { get; set; }
        public BudgeterToken BudgeterToken { get; set; }
    }
}
