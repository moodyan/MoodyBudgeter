using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.RestRequester;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Google
{
    public class GoogleSSOLogic
    {
        private readonly IGoogleOAuthClient GoogleOAuthClient;
        private readonly IRestRequester RestRequester;
        private readonly IEnvironmentRequester EnvironmentRequester;

        public GoogleSSOLogic(IGoogleOAuthClient googleOAuthClient, IRestRequester restRequester, IEnvironmentRequester environmentRequester)
        {
            GoogleOAuthClient = googleOAuthClient;
            RestRequester = restRequester;
            EnvironmentRequester = environmentRequester;
        }

        public async Task<GoogleTokenResponse> VerifyAuthCode(GoogleSSORequest ssoRequest, int portalId)
        {
            string secretKey = EnvironmentRequester.GetVariable("GoogleSSOClientSecret");
            string redirectUrl = EnvironmentRequester.GetVariable("LoyaltyRedirectUrl");
            string googleRequestedScope = EnvironmentRequester.GetVariable("GoogleRequestedScope");

            GoogleTokenResponse tokenResponse = await GoogleOAuthClient.VerifyCode(ssoRequest, secretKey, redirectUrl, googleRequestedScope);
            VerifyTokenBody(tokenResponse);

            return tokenResponse;
        }
        public async Task<GoogleSSOResponse> LoginOrRegisterGoogleUser(GoogleTokenResponse googleTokenResponse, int portalId)
        {
            GoogleUserProfile googleUser = await GoogleOAuthClient.GetUserProfile(googleTokenResponse.AccessToken);

            int userId = await FindExistingUserOrRegister(googleUser, portalId);

            string budgeterClientId = EnvironmentRequester.GetVariable("ServiceClientId");
            BudgeterToken authTokenResponse = await GetAuthTokenForUser(portalId, userId, budgeterClientId, "Google");

            return new GoogleSSOResponse(googleTokenResponse, authTokenResponse);
        }

        public async Task<int> RegisterGoogleUser(GoogleUserProfile googleUser, int portalId)
        {
            // Register the new loyalty user
            RegistrationRequest registrationRequest = new RegistrationRequest
            {
                Username = googleUser.Email,
                RegistrationChannel = "Google-SSO-Provider",
                UserProfileProperties = new List<UserProfileProperty>
                {
                    new UserProfileProperty
                    {
                        ProfilePropertyName = "GoogleId",
                        Value = googleUser.Id
                    },
                    new UserProfileProperty
                    {
                        ProfilePropertyName = "FirstName",
                        Value = googleUser.GivenName
                    },
                    new UserProfileProperty
                    {
                        ProfilePropertyName = "LastName",
                        Value = googleUser.FamilyName
                    },
                    new UserProfileProperty
                    {
                        ProfilePropertyName = "Email",
                        Value = googleUser.Email
                    }
                }
            };

            string registerUserPath = $"/user/{portalId}/v1/user";
            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            RegistrationRequest registrationResponse = await LoyaltyRequester.MakeRequest<RegistrationRequest>(portalId, registerUserPath, HttpMethod.Post, registrationRequest);

            return registrationResponse.UserId;
        }

        public async Task<int> FindExistingUserOrRegister(GoogleUserProfile googleUser, int portalId)
        {
            //Search for user in loyalty by GoogleId PP
            string ppSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Id}&profilepropertyname=GoogleId";
            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            Page<SearchResponse> searchResponse = await LoyaltyRequester.MakeRequest<Page<SearchResponse>>(portalId, ppSearchPath, HttpMethod.Get, null);

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                return searchResponse.Records.Select(x => x.UserId).FirstOrDefault();
            }
            else
            {
                //Search for user in loyalty by Google Email as Username
                string usernameSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Email}&searchusername=true";
                RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
                searchResponse = await LoyaltyRequester.MakeRequest<Page<SearchResponse>>(portalId, usernameSearchPath, HttpMethod.Get, null);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                SearchResponse loyaltyUser = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToLoyaltyPP(loyaltyUser, portalId, googleUser.Id);
                return loyaltyUser.UserId;
            }
            else
            {
                //Search for user in loyalty by Google Email as Email PP
                string usernameSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Email}&profilepropertyname=Email";
                RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
                searchResponse = await LoyaltyRequester.MakeRequest<Page<SearchResponse>>(portalId, usernameSearchPath, HttpMethod.Get, null);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                SearchResponse loyaltyUser = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToLoyaltyPP(loyaltyUser, portalId, googleUser.Id);
                return loyaltyUser.UserId;
            }
            else
            {
                //User does not exist in Loyalty, so Register them
                return await RegisterGoogleUser(googleUser, portalId);
            }
        }

        public async Task AddGoogleIdToProfileProperty(SearchResponse user, int portalId, string googleId)
        {
            List<UserProfileProperty> googleIdPP = new List<UserProfileProperty>
            {
                new UserProfileProperty
                {
                    UserId = user.UserId,
                    SubAccountId = user.SubAccountId,
                    Value = googleId,
                    ProfilePropertyName = "GoogleId"
                }
            };

            string updateUserProfilePropertyPath = $"/user/{portalId}/v1/userprofileproperty";
            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            await LoyaltyRequester.MakeRequest<List<UserProfileProperty>>(portalId, updateUserProfilePropertyPath, HttpMethod.Put, googleIdPP);
        }

        public void VerifyTokenBody(GoogleTokenResponse body)
        {
            if (string.IsNullOrWhiteSpace(body.IdToken) || string.IsNullOrEmpty(body.AccessToken))
            {
                throw new CallerException("An Id Token and Access Token are required");
            }
        }

        public async Task<BudgeterToken> GetLoyaltyAuthTokenForUser(int portalId, int userId, string googleClientId, string provider)
        {
            // reach out to LAPI for access token
            var path = $"/auth/{portalId}/v1/token/provider";
            path += $"?userid={userId}";
            path += $"&provider={provider}";
            path += $"&portalid={portalId}";
            path += $"&audience={googleClientId}";

            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            return await LoyaltyRequester.MakeRequest<LoyaltyToken>(portalId, path, HttpMethod.Get, null);
        }
    }
}