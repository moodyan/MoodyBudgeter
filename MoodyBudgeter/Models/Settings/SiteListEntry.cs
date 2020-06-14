using MoodyBudgeter.Models.Localization;
using MoodyBudgeter.Models.User.Profile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Models.Settings
{
    public class SiteListEntry : ILocalizable
    {
        public int SiteListEntryId { get; set; }
        public int SiteListId { get; set; }
        public string Value { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int UpdatedBy { get; set; }

        public void GetEntityIds(List<LocalizableEntity> localizableEntities)
        {
            localizableEntities.Add(new LocalizableEntity
            {
                EntityId = SiteListEntryId,
                EntityType = "SiteListEntry"
            });
        }

        public void Localize(List<LanguageField> languageFields)
        {
            List<LanguageField> applicableFields = languageFields.Where(c => c.EntityType == "SiteListEntry" && c.EntityId == SiteListEntryId).ToList();

            foreach (var field in applicableFields)
            {
                if (field.Field == "Value")
                {
                    Value = field.Text;
                }
            }
        }
    }
}
