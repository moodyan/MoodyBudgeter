using MoodyBudgeter.Logic.Auth.Password;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Logic.User.Roles;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Registration;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.Settings;
using MoodyBudgeter.Utility.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Registration
{
    public class RegistrationLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly ISettingRequester SettingRequester;
        private readonly AuthContextWrapper AuthContext;
        private readonly UserContextWrapper UserContext;
        private readonly bool IsAdmin;

        public RegistrationLogic(IBudgeterCache cache, IBudgeterLock budgeterLock, AuthContextWrapper authContext, UserContextWrapper userContext, ISettingRequester settingRequester, bool isAdmin)
        {
            Cache = cache;
            BudgeterLock = budgeterLock;
            SettingRequester = settingRequester;
            AuthContext = authContext;
            UserContext = userContext;
            IsAdmin = isAdmin;
        }

        public async Task<RegistrationRequest> RegisterUser(RegistrationRequest registrationRequest)
        {
            await ValidateForm(registrationRequest);

            var registrationType = await GetUserRegistrationType();

            await IsUserEnabled(registrationType);

            var userProfileUpdater = new UserProfileUpdater(Cache, BudgeterLock, UserContext);

            // Validate the profile first, we don't want to create the user, and then fail registration.
            // TODO: Thread safety. I think we have to wait until we can delete users to handle this property, check, register, then save profile, if profile fails delete user.
            var validatedProfile = await userProfileUpdater.ValidateUserProfilePropertyUpdates(registrationRequest.UserProfileProperties, IsAdmin, true);

            var user = await new UserLogic(Cache, UserContext).CreateUser(new BudgetUser { Username = registrationRequest.Username });

            registrationRequest.UserId = user.UserId;

            await SaveProfile(registrationRequest, validatedProfile, userProfileUpdater);

            await GiveUserRoles(registrationRequest.UserId);

            if (registrationRequest.CreateRegistrationToken)
            {
                registrationRequest.RegistrationToken = await CreateRegistrationToken(registrationRequest);
            }

            //await SendRegistrationEvent(registrationRequest, registrationType);

            return registrationRequest;
        }

        private async Task ValidateForm(RegistrationRequest registrationRequest)
        {
            if (registrationRequest == null)
            {
                throw new CallerException("Registration Form Cannot be null.");
            }

            if (string.IsNullOrEmpty(registrationRequest.Username))
            {
                throw new CallerException("Username field is required.");
            }

            if (registrationRequest.Username.Any(char.IsWhiteSpace))
            {
                throw new CallerException("Username may not contain spaces.");
            }

            var usernameLogic = new UsernameLogic(Cache, SettingRequester, UserContext);

            await usernameLogic.ValidateUsername(registrationRequest.Username);

            if (string.IsNullOrEmpty(registrationRequest.RegistrationChannel))
            {
                throw new CallerException("Registration Channel is required.");
            }
        }

        private async Task<bool> IsUserEnabled(RegistrationType type)
        {
            switch (type)
            {
                case RegistrationType.None:
                    throw new CallerException("Registration is set to none");
                case RegistrationType.Private:
                    return IsAdmin;
                case RegistrationType.Public:
                    return true;
                case RegistrationType.Verified:
                    if (!IsAdmin)
                    {
                        return false;
                    }

                    // The text for this setting is 'Auto-verify admin 'Verified' registrations'. This means, if true, if an admin registers they will not have to verify
                    return await GetSetting("SystemSettings_VerifyAdminRegistrations");
                default:
                    throw new NotSupportedException("RegistrationType not supported");
            }
        }

        private async Task<RegistrationType> GetUserRegistrationType()
        {
            var registrationSetting = await SettingRequester.GetSetting("SystemSettings_UserRegistration");

            if (int.TryParse(registrationSetting, out int type))
            {
                return (RegistrationType)type;
            }
            else
            {
                throw new CallerException("Registration type not set.");
            }
        }

        private async Task<bool> GetSetting(string name)
        {
            var setting = await SettingRequester.GetSetting(name);

            if (bool.TryParse(setting, out bool value))
            {
                return value;
            }
            else
            {
                return false;
            }
        }

        private async Task GiveUserRoles(int userId)
        {
            var roleLogic = new RoleLogic(Cache, UserContext);

            var roles = await roleLogic.GetAutoRoles();

            var userRoleUpdater = new UserRoleUpdater(Cache, UserContext);

            foreach (var role in roles)
            {
                await userRoleUpdater.AddRoleToUser(new UserRole { RoleId = role.RoleId, UserId = userId });
            }
        }

        private async Task SaveProfile(RegistrationRequest registrationRequest, Dictionary<UserProfileProperty, ProfileProperty> validatedProfile, UserProfileUpdater userProfileUpdater)
        {
            if (registrationRequest.UserProfileProperties == null || registrationRequest.UserProfileProperties.Count == 0)
            {
                return;
            }

            foreach (var profileProperty in validatedProfile)
            {
                profileProperty.Key.UserId = registrationRequest.UserId;
            }

            registrationRequest.UserProfileProperties = await userProfileUpdater.SaveValidatedUserProfileProperties(validatedProfile, IsAdmin);
        }

        private async Task<string> CreateRegistrationToken(RegistrationRequest registrationRequest)
        {
            var passwordResetLogic = new PasswordResetLogic(Cache, AuthContext, UserContext);
            string token = await passwordResetLogic.CreateEmptyCredentialsWithResetToken(registrationRequest.UserId, registrationRequest.Username);

            return token;
        }

        //private async Task SendRegistrationEvent(RegistrationRequest registrationRequest, RegistrationType type)
        //{
        //    var message = new UserRegistered
        //    {
        //        UserId = registrationRequest.UserId,
        //        AdminRegistration = IsAdmin,
        //        CodeChallenge = registrationRequest.CodeChallenge
        //    };

        //    await WhatEmailShouldWeSend(registrationRequest, type, message);

        //    await QueueSender.SendMessage<UserRegistered>(message);
        //}

        //private async Task WhatEmailShouldWeSend(RegistrationRequest registrationRequest, RegistrationType type, UserRegistered message)
        //{
        //    // We might need to send a verified email if the caller wants a registration token (to make a login)
        //    if (type == RegistrationType.Verified && registrationRequest.CreateRegistrationToken)
        //    {
        //        if (!string.IsNullOrEmpty(registrationRequest.RegistrationToken))
        //        {
        //            message.RegistrationToken = registrationRequest.RegistrationToken;
        //        }

        //        if (!IsAdmin)
        //        {
        //            message.SendVerificationEmail = true;
        //            registrationRequest.RegistrationToken = "";
        //        }
        //        else
        //        {
        //            // if we don't auto verify then we might need to send verification email, if we do then might send registration email.
        //            if (!await GetSetting("SystemSettings_VerifyAdminRegistrations"))
        //            {
        //                message.SendVerificationEmail = true;
        //                registrationRequest.RegistrationToken = "";
        //            }
        //        }
        //    }

        //    // We respect the override first, the caller can decide if they get the email or not.
        //    if (registrationRequest.SendRegistrationEmail.HasValue)
        //    {
        //        message.SendRegistrationEmail = registrationRequest.SendRegistrationEmail.Value;
        //        return;
        //    }

        //    if (IsAdmin)
        //    {
        //        if (await GetSetting("SystemSettings_SendAdminRegistrationEmail"))
        //        {
        //            message.SendRegistrationEmail = true;
        //        }

        //        return;
        //    }

        //    if (await GetSetting("SystemSettings_SendRegistrationEmail"))
        //    {
        //        message.SendRegistrationEmail = true;
        //    }
        //}
    }
}
