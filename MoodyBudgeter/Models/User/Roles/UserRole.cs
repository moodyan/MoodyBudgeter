using MoodyBudgeter.Models.Localization;
using MoodyBudgeter.Models.User.Profile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Models.User.Roles
{
    public class UserRole : ILocalizable
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public string RoleName { get; set; }
        public int Ordinal { get; set; }

        public void GetEntityIds(List<LocalizableEntity> localizableEntities)
        {
            localizableEntities.Add(new LocalizableEntity
            {
                EntityId = RoleId,
                EntityType = "Roles"
            });
        }

        public void Localize(List<LanguageField> languageFields)
        {
            var applicableFields = languageFields.Where(c => c.EntityType == "Roles" && c.EntityId == RoleId).ToList();

            foreach (var field in applicableFields)
            {
                if (field.Field == "RoleName")
                {
                    RoleName = field.Text;
                }
            }
        }
    }
}
