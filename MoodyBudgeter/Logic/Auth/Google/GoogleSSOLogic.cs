using MoodyBudgeter.Logic.User.Registration;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Registration;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.RestRequester;
using MoodyBudgeter.Utility.Lock;
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
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly ContextWrapper Context;

        public GoogleSSOLogic(IGoogleOAuthClient googleOAuthClient, IRestRequester restRequester, IEnvironmentRequester environmentRequester, IBudgeterCache cache, ContextWrapper context, IBudgeterLock budgeterLock)
        {
            GoogleOAuthClient = googleOAuthClient;
            RestRequester = restRequester;
            EnvironmentRequester = environmentRequester;
            Cache = cache;
            Context = context;
            BudgeterLock = budgeterLock;
        }

        public async Task<GoogleTokenResponse> VerifyAuthCode(GoogleSSORequest ssoRequest)
        {
            string secretKey = EnvironmentRequester.GetVariable("GoogleSSOClientSecret");
            string redirectUrl = EnvironmentRequester.GetVariable("LoyaltyRedirectUrl");
            string googleRequestedScope = EnvironmentRequester.GetVariable("GoogleRequestedScope");

            GoogleTokenResponse tokenResponse = await GoogleOAuthClient.VerifyCode(ssoRequest, secretKey, redirectUrl, googleRequestedScope);
            VerifyTokenBody(tokenResponse);

            return tokenResponse;
        }
        public async Task<GoogleSSOResponse> LoginOrRegisterGoogleUser(GoogleTokenResponse googleTokenResponse, bool isAdmin)
        {
            GoogleUserProfile googleUser = await GoogleOAuthClient.GetUserProfile(googleTokenResponse.AccessToken);

            int userId = await FindExistingUserOrRegister(googleUser, isAdmin);

            string budgeterClientId = EnvironmentRequester.GetVariable("ServiceClientId");
            BudgeterToken authTokenResponse = await GetAuthTokenForUser(userId, budgeterClientId, "Google");

            return new GoogleSSOResponse(googleTokenResponse, authTokenResponse);
        }

        public async Task<int> RegisterGoogleUser(GoogleUserProfile googleUser, bool isAdmin)
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

            RegistrationLogic registrationLogic = new RegistrationLogic(Cache, BudgeterLock, Context, isAdmin);

            RegistrationRequest registrationResponse = await registrationLogic.RegisterUser(registrationRequest);
            return registrationResponse.UserId;
        }

        public async Task<int> FindExistingUserOrRegister(GoogleUserProfile googleUser, bool isAdmin)
        {
            //Search for user in loyalty by GoogleId PP
            string ppSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Id}&profilepropertyname=GoogleId";
            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            Page<UserSearchResponse> searchResponse = await LoyaltyRequester.MakeRequest<Page<UserSearchResponse>>(ppSearchPath, HttpMethod.Get, null);

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                return searchResponse.Records.Select(x => x.UserId).FirstOrDefault();
            }
            else
            {
                //Search for user in loyalty by Google Email as Username
                string usernameSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Email}&searchusername=true";
                RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
                searchResponse = await LoyaltyRequester.MakeRequest<Page<UserSearchResponse>>(usernameSearchPath, HttpMethod.Get, null);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                UserSearchResponse user = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToProfileProperty(user, googleUser.Id);
                return user.UserId;
            }
            else
            {
                //Search for user in loyalty by Google Email as Email PP
                string usernameSearchPath = $"/user/{portalId}/v1/search?searchtext={googleUser.Email}&profilepropertyname=Email";
                RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
                searchResponse = await LoyaltyRequester.MakeRequest<Page<UserSearchResponse>>(usernameSearchPath, HttpMethod.Get, null);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                UserSearchResponse loyaltyUser = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToProfileProperty(loyaltyUser, googleUser.Id);
                return loyaltyUser.UserId;
            }
            else
            {
                //User does not exist in Loyalty, so Register them
                return await RegisterGoogleUser(googleUser, isAdmin);
            }
        }

        public async Task AddGoogleIdToProfileProperty(UserSearchResponse user, string googleId)
        {
            List<UserProfileProperty> googleIdPP = new List<UserProfileProperty>
            {
                new UserProfileProperty
                {
                    UserId = user.UserId,
                    Value = googleId,
                    ProfilePropertyName = "GoogleId"
                }
            };

            string updateUserProfilePropertyPath = $"/user/{portalId}/v1/userprofileproperty";
            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            await LoyaltyRequester.MakeRequest<List<UserProfileProperty>>(updateUserProfilePropertyPath, HttpMethod.Put, googleIdPP);
        }

        public void VerifyTokenBody(GoogleTokenResponse body)
        {
            if (string.IsNullOrWhiteSpace(body.IdToken) || string.IsNullOrEmpty(body.AccessToken))
            {
                throw new CallerException("An Id Token and Access Token are required");
            }
        }

        public async Task<BudgeterToken> GetAuthTokenForUser(int userId, string googleClientId, string provider)
        {
            // reach out to LAPI for access token
            var path = $"/auth/{portalId}/v1/token/provider";
            path += $"?userid={userId}";
            path += $"&provider={provider}";
            path += $"&portalid={portalId}";
            path += $"&audience={googleClientId}";

            RestRequester.BaseUrl = EnvironmentRequester.GetVariable("GatewayBase");
            return await LoyaltyRequester.MakeRequest<LoyaltyToken>(path, HttpMethod.Get, null);
        }
    }
}