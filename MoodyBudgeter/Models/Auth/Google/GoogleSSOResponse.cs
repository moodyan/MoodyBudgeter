namespace MoodyBudgeter.Models.Auth.Google
{
    public class GoogleSSOResponse
    {
        public GoogleSSOResponse(GoogleTokenResponse googleToken, BudgeterToken loyaltyToken)
        {
            GoogleToken = googleToken;
            BudgeterToken = loyaltyToken;
        }

        public GoogleTokenResponse GoogleToken { get; set; }
        public BudgeterToken BudgeterToken { get; set; }
    }
}
