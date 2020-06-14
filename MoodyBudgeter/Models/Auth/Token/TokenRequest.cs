namespace MoodyBudgeter.Models.Auth
{
    public class TokenRequest
    {
        public string GrantType { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string RefreshToken { get; set; }
        public string Code { get; set; }
        public string CodeVerifier { get; set; }
        public bool SuperUserRequest { get; set; }
    }
}
