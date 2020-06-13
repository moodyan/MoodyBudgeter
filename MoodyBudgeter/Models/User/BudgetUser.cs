using MoodyBudgeter.Models.Localization;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Roles;
using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Models.User
{
    public class BudgetUser : ILocalizable
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public bool Enabled { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<UserProfileProperty> UserProfileProperties { get; set; }
        public List<UserRole> UserRoles { get; set; }

        public string Email { get; set; }

        public void GetEntityIds(List<LocalizableEntity> localizableEntities)
        {
            foreach (ILocalizable userProfileProperty in UserProfileProperties)
            {
                userProfileProperty.GetEntityIds(localizableEntities);
            }

            foreach (ILocalizable userRoles in UserRoles)
            {
                userRoles.GetEntityIds(localizableEntities);
            }
        }

        public void Localize(List<LanguageField> languageFields)
        {
            foreach (ILocalizable userProfileProperty in UserProfileProperties)
            {
                userProfileProperty.Localize(languageFields);
            }
        }
    }
}
