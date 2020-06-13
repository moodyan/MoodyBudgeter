using MoodyBudgeter.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Models.User.Profile
{
    public class UserProfileProperty : ILocalizable
    {
        public int UserProfilePropertyId { get; set; }
        public int UserId { get; set; }
        public int ProfilePropertyId { get; set; }
        public string Value { get; set; }
        public DateTime DateUpdated { get; set; }
        public string ProfilePropertyName { get; set; }
        public string ProfilePropertyLabel { get; set; }
        public int Ordinal { get; set; }
        
        public void GetEntityIds(List<LocalizableEntity> localizableEntities)
        {
            localizableEntities.Add(new LocalizableEntity
            {
                EntityId = ProfilePropertyId,
                EntityType = "ProfilePropertyDefinition"
            });
        }

        public void Localize(List<LanguageField> languageFields)
        {
            var applicableFields = languageFields.Where(c => c.EntityType == "ProfilePropertyDefinition" && c.EntityId == ProfilePropertyId).ToList();

            foreach (var field in applicableFields)
            {
                if (field.Field == "Title")
                {
                    ProfilePropertyName = field.Text;
                }
            }
        }
    }

    public interface ILocalizable
    {
        void GetEntityIds(List<LocalizableEntity> localizableEntities);
        void Localize(List<LanguageField> languageFields);
    }
}
