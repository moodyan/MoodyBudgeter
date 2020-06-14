using MoodyBudgeter.Logic.Auth.Token;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Logic.User.Registration;
using MoodyBudgeter.Logic.User.Search;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Google;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Registration;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.EnvironmentRequester;
using MoodyBudgeter.Utility.Clients.GoogleAuth;
using MoodyBudgeter.Utility.Clients.Settings;
using MoodyBudgeter.Utility.Lock;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth.Google
{
    public class GoogleSSOLogic
    {
        private readonly IGoogleOAuthClient GoogleOAuthClient;
        private readonly IEnvironmentRequester EnvironmentRequester;
        private readonly ISettingRequester SettingRequester;
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly UserContextWrapper UserContext;
        private readonly AuthContextWrapper AuthContext;

        public GoogleSSOLogic(IGoogleOAuthClient googleOAuthClient, IEnvironmentRequester environmentRequester, ISettingRequester settingRequester, IBudgeterCache cache, UserContextWrapper userContext, AuthContextWrapper authContext, IBudgeterLock budgeterLock)
        {
            GoogleOAuthClient = googleOAuthClient;
            EnvironmentRequester = environmentRequester;
            SettingRequester = settingRequester;
            Cache = cache;
            UserContext = userContext;
            AuthContext = authContext;
            BudgeterLock = budgeterLock;
        }

        public async Task<GoogleTokenResponse> VerifyAuthCode(GoogleSSORequest ssoRequest)
        {
            string secretKey = EnvironmentRequester.GetVariable("GoogleSSOClientSecret");
            string redirectUrl = EnvironmentRequester.GetVariable("BudgeterRedirectUrl");
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

            RegistrationLogic registrationLogic = new RegistrationLogic(Cache, BudgeterLock, AuthContext, UserContext, SettingRequester, isAdmin);

            RegistrationRequest registrationResponse = await registrationLogic.RegisterUser(registrationRequest);
            return registrationResponse.UserId;
        }

        public async Task<int> FindExistingUserOrRegister(GoogleUserProfile googleUser, bool isAdmin)
        {
            //Search for user by GoogleId PP
            var searchLogic = new SearchLogic(Cache, UserContext);

            var search = new UserSearch
            {
                SearchText = googleUser.Id,
                ProfilePropertyName = "GoogleId"
            };

            Page<UserSearchResponse> searchResponse = await searchLogic.Search(search);

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                return searchResponse.Records.Select(x => x.UserId).FirstOrDefault();
            }
            else
            {
                //Search for user by Google Email as Username
                var usernameSearch = new UserSearch
                {
                    SearchText = googleUser.Email,
                    SearchUsername = true
                };

                searchResponse = await searchLogic.Search(usernameSearch);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                UserSearchResponse user = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToProfileProperty(user, googleUser.Id, isAdmin);
                return user.UserId;
            }
            else
            {
                //Search for user by Google Email as Email PP
                var emailSearch = new UserSearch
                {
                    SearchText = googleUser.Email,
                    ProfilePropertyName = "Email"
                };

                searchResponse = await searchLogic.Search(emailSearch);
            }

            if (searchResponse != null && searchResponse.Records != null && searchResponse.Records.Count > 0)
            {
                UserSearchResponse user = searchResponse.Records.FirstOrDefault();
                await AddGoogleIdToProfileProperty(user, googleUser.Id, isAdmin);
                return user.UserId;
            }
            else
            {
                //User does not exist, so Register them
                return await RegisterGoogleUser(googleUser, isAdmin);
            }
        }

        public async Task AddGoogleIdToProfileProperty(UserSearchResponse user, string googleId, bool isAdmin)
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
            
            UserProfileUpdater userProfileUpdater = new UserProfileUpdater(Cache, BudgeterLock, UserContext);

            await userProfileUpdater.UpdateUserProfileProperties(googleIdPP, isAdmin);
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
            var providerTokenLogic = new FederatedIdentityTokenLogic(EnvironmentRequester, UserContext, AuthContext, Cache);

            return await providerTokenLogic.IssueFederatedIdentityToken(userId, googleClientId, provider);
        }
    }
}