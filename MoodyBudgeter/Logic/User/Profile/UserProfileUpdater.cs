using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Profile
{
    public class UserProfileUpdater
    {
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly UserContextWrapper Context;

        public UserProfileUpdater(IBudgeterCache cache, IBudgeterLock budgeterLock, UserContextWrapper context)
        {
            Cache = cache;
            BudgeterLock = budgeterLock;
            Context = context;
        }

        public async Task<List<UserProfileProperty>> UpdateUserProfileProperties(List<UserProfileProperty> userProfileProperties, bool isAdmin)
        {
            var tasks = (from userProfileProperty in userProfileProperties
                         select UpdateUserProfileProperty(userProfileProperty, isAdmin)).ToList();
            
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<UserProfileProperty> UpdateUserProfileProperty(UserProfileProperty userProfileProperty, bool isAdmin)
        {
            var profileProperty = await ValidateUserProfilePropertyUpdate(userProfileProperty, isAdmin);

            return await UpdateUserProfileProperty(userProfileProperty, profileProperty, isAdmin, false);
        }

        public async Task<List<UserProfileProperty>> SaveValidatedUserProfileProperties(Dictionary<UserProfileProperty, ProfileProperty> properties, bool isAdmin)
        {
            if (properties == null)
            {
                return null;
            }

            var tasks = from property in properties
                        select UpdateUserProfileProperty(property.Key, property.Value, isAdmin, true);

            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task<UserProfileProperty> UpdateUserProfileProperty(UserProfileProperty userProfileProperty, ProfileProperty profileProperty, bool isAdmin, bool uniqueChecked)
        {
            await ValidateUser(userProfileProperty);

            var userProfilePropertyLogic = new UserProfilePropertyLogic(Cache, Context);

            var existingUserProfileProperty = await userProfilePropertyLogic.GetUserProfileProperty(userProfileProperty.UserId, userProfileProperty.ProfilePropertyId, isAdmin);

            UserProfileProperty updatedUserProfileProperty = null;

            var evaluateUnique = ShouldValidateUniqueness(userProfileProperty, profileProperty, uniqueChecked);

            string lockName = "UpdateUserProfileProperty-ProfilePropertyId-" + profileProperty.ProfilePropertyId;

            if (existingUserProfileProperty != null && existingUserProfileProperty.UserProfilePropertyId > 0)
            {
                updatedUserProfileProperty = await ProcessExistingProfileProperty(existingUserProfileProperty, userProfileProperty, profileProperty, evaluateUnique, lockName);
            }
            else
            {
                updatedUserProfileProperty = await ProcessNewProfileProperty(existingUserProfileProperty, userProfileProperty, profileProperty, evaluateUnique, lockName, userProfilePropertyLogic, isAdmin);
            }

            //await SendUserProfilePropertyUpdatedEvent(updatedUserProfileProperty);

            await new UserProfilePropertyCache(Cache).InvalidateUserProfilePropertiesCache(updatedUserProfileProperty.UserId);

            return updatedUserProfileProperty;
        }

        private async Task<UserProfileProperty> UpdateExistingProfileProperty(UserProfileProperty userProfileProperty, UserProfileProperty existingUserProfileProperty, ProfileProperty profileProperty, bool evaluateUnique, string lockName)
        {
            existingUserProfileProperty.Value = userProfileProperty.Value;

            UserProfileProperty updated;

            if (evaluateUnique)
            {
                using (await BudgeterLock.Lock(lockName))
                {
                    await ValidateUniqueness(userProfileProperty, profileProperty);

                    updated = await UpdateExistingProfilePropertyInDb(existingUserProfileProperty);
                }
            }
            else
            {
                updated = await UpdateExistingProfilePropertyInDb(existingUserProfileProperty);
            }

            existingUserProfileProperty.DateUpdated = updated.DateUpdated;

            return existingUserProfileProperty;
        }

        private async Task CreateProfileProperty(UserProfileProperty userProfileProperty, ProfileProperty profileProperty, bool evaluateUnique, string lockName)
        {
            if (evaluateUnique)
            {
                using (await BudgeterLock.Lock(lockName))
                {
                    await ValidateUniqueness(userProfileProperty, profileProperty);

                    await CreateProfilePropertyInDb(userProfileProperty, profileProperty.Visibility);
                }
            }
            else
            {
                await CreateProfilePropertyInDb(userProfileProperty, profileProperty.Visibility);
            }

            await new UserProfilePropertyCache(Cache).InvalidateUserProfilePropertiesCache(userProfileProperty.UserId);
        }

        private async Task<UserProfileProperty> UpdateExistingProfilePropertyInDb(UserProfileProperty updateUserProfileProperty)
        {
            UserProfileProperty updatedProfile;
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                updatedProfile = await repo.Update(updateUserProfileProperty);
            }

            await new UserProfilePropertyCache(Cache).InvalidateUserProfilePropertiesCache(updatedProfile.UserId);

            return updatedProfile;
        }
        
        private async Task CreateProfilePropertyInDb(UserProfileProperty userProfileProperty, ProfilePropertyVisibility visibility)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                await repo.CreateWithData(userProfileProperty, visibility);
            }
        }

        public async Task<Dictionary<UserProfileProperty, ProfileProperty>> ValidateUserProfilePropertyUpdates(List<UserProfileProperty> userProfileProperties, bool isAdmin, bool isRegistration)
        {
            var validatedProfile = new Dictionary<UserProfileProperty, ProfileProperty>();

            if (isRegistration)
            {
                await ValidatedRequiredProfileProperties(userProfileProperties);
            }

            if (userProfileProperties == null || userProfileProperties.Count == 0)
            {
                return null;
            }

            foreach (var userProfileProperty in userProfileProperties)
            {
                var profileProperty = await ValidateUserProfilePropertyUpdate(userProfileProperty, isAdmin);

                if (ShouldValidateUniqueness(userProfileProperty, profileProperty, false))
                {
                    await ValidateUniqueness(userProfileProperty, profileProperty);
                }

                validatedProfile.Add(userProfileProperty, profileProperty);
            }

            return validatedProfile;
        }

        private async Task ValidatedRequiredProfileProperties(List<UserProfileProperty> userProfileProperties)
        {
            List<ProfileProperty> requiredProfileProperties = await new ProfilePropertyLogic(Cache, Context).GetProfileProperties(true, true);

            if (requiredProfileProperties == null || requiredProfileProperties.Count == 0)
            {
                return;
            }

            if (userProfileProperties == null || userProfileProperties.Count == 0)
            {
                throw new CallerException("Required ProfileProperty " + requiredProfileProperties.FirstOrDefault().Name + " is missing");
            }

            var userProfilePropertiesWithValue = userProfileProperties.Where(c => !string.IsNullOrEmpty(c.Value)).ToList();

            foreach (ProfileProperty requiredProfileProperty in requiredProfileProperties)
            {
                if (userProfilePropertiesWithValue.Any(c => c.ProfilePropertyId == requiredProfileProperty.ProfilePropertyId))
                {
                    continue;
                }

                if (userProfilePropertiesWithValue.Any(c => c.ProfilePropertyName != null && c.ProfilePropertyName.ToUpper() == requiredProfileProperty.Name.ToUpper()))
                {
                    continue;
                }

                throw new CallerException("Required ProfileProperty " + requiredProfileProperty.Name + " is missing");
            }
        }

        private async Task<ProfileProperty> ValidateUserProfilePropertyUpdate(UserProfileProperty userProfileProperty, bool isAdmin)
        {
            if (userProfileProperty == null)
            {
                throw new CallerException("No Profile Property data provided.");
            }

            if (userProfileProperty.ProfilePropertyId <= 0 && string.IsNullOrEmpty(userProfileProperty.ProfilePropertyName))
            {
                throw new CallerException("A ProfilePropertyName or ProfilePropertyId is required.");
            }

            ProfileProperty profileProperty;

            ProfilePropertyLogic profilePropertyLogic = new ProfilePropertyLogic(Cache, Context);

            if (userProfileProperty.ProfilePropertyId <= 0)
            {
                profileProperty = await profilePropertyLogic.GetProfileProperty(userProfileProperty.ProfilePropertyName, isAdmin);
            }
            else
            {
                profileProperty = await profilePropertyLogic.GetProfileProperty(userProfileProperty.ProfilePropertyId, isAdmin);
            }

            if (profileProperty == null)
            {
                throw new CallerException("ProfileProperty could not be found.");
            }

            userProfileProperty.ProfilePropertyName = profileProperty.Name;
            userProfileProperty.ProfilePropertyId = profileProperty.ProfilePropertyId;

            if (!isAdmin && profileProperty.Visibility == ProfilePropertyVisibility.Admin)
            {
                throw new CallerException("User cannot modify admin only profile property");
            }

            ConfirmRegexRequirements(userProfileProperty, profileProperty);

            return profileProperty;
        }

        private void ConfirmRegexRequirements(UserProfileProperty userProfileProperty, ProfileProperty profileProperty)
        {
            if (string.IsNullOrEmpty(profileProperty.PcreRegex))
            {
                return;
            }

            var match = Regex.Match(userProfileProperty.Value, profileProperty.PcreRegex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                throw new FriendlyException("UserProfileProperty.RegexRequirementsNotMet", userProfileProperty.Value + " does not meet regex requirements");
            }
        }

        private async Task ValidateUser(UserProfileProperty userProfileProperty)
        {
            if (userProfileProperty.UserId <= 0)
            {
                throw new CallerException("Valid UserId is required.");
            }

            await new UserLogic(Cache, Context).GetUserWithoutRelated(userProfileProperty.UserId);
        }

        private bool ShouldValidateUniqueness(UserProfileProperty userProfileProperty, ProfileProperty profileProperty, bool uniqueChecked)
        {
            if (uniqueChecked)
            {
                return false;
            }

            if (string.IsNullOrEmpty(userProfileProperty.Value))
            {
                return false;
            }

            if (!profileProperty.Unique)
            {
                return false;
            }

            return true;
        }

        private async Task ValidateUniqueness(UserProfileProperty userProfileProperty, ProfileProperty profileProperty)
        {
            UserProfilePropertyLogic userProfilePropertyLogic = new UserProfilePropertyLogic(Cache, Context);

            List<int> users = await userProfilePropertyLogic.FindUsersFromValue(profileProperty.ProfilePropertyId, userProfileProperty.Value);

            if (users != null && users.Count > 0)
            {
                throw new FriendlyException("UserProfileProperty.ValueAlreadyTaken", profileProperty.Name + " is already taken by another user");
            }
        }

        //private async Task SendUserProfilePropertyUpdatedEvent(UserProfileProperty userProfileProperty)
        //{
        //    var message = new UserProfilePropertyUpdated
        //    {
        //        UserProfilePropertyId = userProfileProperty.UserProfilePropertyId,
        //        UserId = userProfileProperty.UserId,
        //        ProfilePropertyId = userProfileProperty.ProfilePropertyId,
        //        ProfilePropertyName = userProfileProperty.ProfilePropertyName,
        //        Level = Level
        //    };

        //    await QueueSender.SendMessage<UserProfilePropertyUpdated>(message);
        //}

        private async Task<UserProfileProperty> ProcessExistingProfileProperty(UserProfileProperty existingUserProfileProperty, UserProfileProperty userProfileProperty, ProfileProperty profileProperty, bool evaluateUnique, string lockName)
        {
            if (userProfileProperty.DateUpdated != default && existingUserProfileProperty.DateUpdated > userProfileProperty.DateUpdated)
            {
                // Don't update if LastUpdated is before the existing.
                return existingUserProfileProperty;
            }

            if (existingUserProfileProperty.Value == userProfileProperty.Value)
            {
                // Values match, don't update.
                return existingUserProfileProperty;
            }

            // Cascade email profile update to user email
            if (existingUserProfileProperty.ProfilePropertyName.ToLower() == "email")
            {
                BudgetUser cascadeCall = null;
                cascadeCall = await CascadeEmail(userProfileProperty);
                if (cascadeCall != null)
                {
                    var updatedUserProfileProperty = await UpdateExistingProfileProperty(userProfileProperty,
                    existingUserProfileProperty, profileProperty, evaluateUnique, lockName);
                    return updatedUserProfileProperty;
                }
                else
                {
                    // cascade failed, stop and return existing property
                    return existingUserProfileProperty;
                }
            }
            else
            {
                var updatedUserProfileProperty = await UpdateExistingProfileProperty(userProfileProperty,
                    existingUserProfileProperty, profileProperty, evaluateUnique, lockName);
                return updatedUserProfileProperty;
            }
        }

        private async Task<UserProfileProperty> ProcessNewProfileProperty(UserProfileProperty existingUserProfileProperty, UserProfileProperty userProfileProperty, ProfileProperty profileProperty, bool evaluateUnique, string lockName, UserProfilePropertyLogic userProfilePropertyLogic, bool isAdmin)
        {
            if (string.IsNullOrEmpty(userProfileProperty.Value))
            {
                // If the update is empty and the user profile property doesn't exist return a blank profile entry.
                return existingUserProfileProperty;
            }
            if (existingUserProfileProperty.ProfilePropertyName.ToLower() == "email")
            {
                BudgetUser cascadeCall = null;
                cascadeCall = await CascadeEmail(userProfileProperty);
                // cascade call failed, return
                if (cascadeCall == null)
                {
                    return existingUserProfileProperty;
                }
                await CreateProfileProperty(userProfileProperty, profileProperty, evaluateUnique, lockName);

            }
            else
            {
                await CreateProfileProperty(userProfileProperty, profileProperty, evaluateUnique, lockName);
            }

            // Now that the property exists, grab it with definition info.
            UserProfileProperty updatedUserProfileProperty = await userProfilePropertyLogic.GetUserProfileProperty(userProfileProperty.UserId, userProfileProperty.ProfilePropertyId, isAdmin);

            return updatedUserProfileProperty;
        }

        private async Task<BudgetUser> CascadeEmail(UserProfileProperty userProfileProperty)
        {
            BudgetUser getUser = null;
            BudgetUser updatedUser = null;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);
                getUser = await repo.Find(userProfileProperty.UserId);
                if (getUser != null)
                {
                    getUser.Email = userProfileProperty.Value;
                    updatedUser = await repo.Update(getUser);
                }
                return updatedUser;
            }
        }
    }
}
