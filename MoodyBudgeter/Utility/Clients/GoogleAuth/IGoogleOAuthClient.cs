using MoodyBudgeter.Models.Auth.Google;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.GoogleAuth
{
    public interface IGoogleOAuthClient
    {
        Task<GoogleTokenResponse> VerifyCode(GoogleSSORequest ssoRequest, string secretKey, string redirectUrl, string googleRequestedScope);
        Task<GoogleUserProfile> GetUserProfile(string accesstoken);
    }
}
