namespace MoodyBudgeter.Models.Auth.Token
{
    public enum TokenType
    {
        Implicit,
        AuthCode,
        Refresh,
        RefreshAccess,
        ClientCredentials,
        Federated,
        Identity
    }
}
