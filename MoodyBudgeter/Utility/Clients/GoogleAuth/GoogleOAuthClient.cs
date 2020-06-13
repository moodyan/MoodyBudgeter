using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Utility.Clients.RestRequester;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Clients.GoogleAuth
{
    public class GoogleOAuthClient : IGoogleOAuthClient
    {
        private readonly IRestRequester RestRequester;

        public GoogleOAuthClient(IRestRequester restRequester)
        {
            RestRequester = restRequester;
        }

        public async Task<GoogleTokenResponse> VerifyCode(GoogleSSORequest ssoRequest, string secretKey, string redirectUrl, string googleRequestedScope)
        {
            if (string.IsNullOrEmpty(ssoRequest.AuthorizationCode) || string.IsNullOrEmpty(ssoRequest.ClientId))
            {
                throw new CallerException("Login code and ClientId cannot be null or empty.");
            }

            RestRequester.BaseUrl = "https://oauth2.googleapis.com";
            RestRequester.RequestContentType = "application/x-www-form-urlencoded";
            string path = $"/token?code={ssoRequest.AuthorizationCode}&redirect_uri={redirectUrl}&client_id={ssoRequest.ClientId}&client_secret={secretKey}&scope={googleRequestedScope}&grant_type=authorization_code";
            return await RestRequester.MakeRequest<GoogleTokenResponse>(path, HttpMethod.Post, null);
        }

        public async Task<GoogleUserProfile> GetUserProfile(string accesstoken)
        {
            RestRequester.BaseUrl = "https://www.googleapis.com";
            RestRequester.RequestContentType = "application/x-www-form-urlencoded";
            string path = $"/oauth2/v1/userinfo?alt=json&access_token={accesstoken}";
            return await RestRequester.MakeRequest<GoogleUserProfile>(path, HttpMethod.Get, null);
        }
    }
}
