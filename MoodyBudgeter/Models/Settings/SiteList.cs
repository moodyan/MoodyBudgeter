using MoodyBudgeter.Models.Localization;
using MoodyBudgeter.Models.User.Profile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Models.Settings
{
    public class SiteList : ILocalizable
    {
        public int SiteListId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int UpdatedBy { get; set; }
        public bool Visible { get; set; }

        public void GetEntityIds(List<LocalizableEntity> localizableEntities)
        {
            localizableEntities.Add(new LocalizableEntity
            {
                EntityId = SiteListId,
                EntityType = "SiteList"
            });
        }

        public void Localize(List<LanguageField> languageFields)
        {
            var applicableFields = languageFields.Where(c => c.EntityType == "SiteList" && c.EntityId == SiteListId).ToList();

            foreach (var field in applicableFields)
            {
                if (field.Field == "Name")
                {
                    Name = field.Text;
                }
            }
        }
    }
}
