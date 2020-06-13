using MoodyBudgeter.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Models.User.Profile
{
    public class ProfileProperty : ILocalizable
    {
        public int ProfilePropertyId { get; set; }
        public string PropertyCategory { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string InputMask { get; set; }
        public bool Required { get; set; }
        public ProfilePropertyVisibility Visibility { get; set; }
        public string PcreRegex { get; set; }
        public string JsRegex { get; set; }
        public bool Unique { get; set; }
        public int Ordinal { get; set; }
        public DataType DataType { get; set; }
        public int? ListId { get; set; }
        public DateTime DateCreated { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }

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
                    Name = field.Text;
                }

                if (field.Field == "Label")
                {
                    Label = field.Text;
                }

                if (field.Field == "Description")
                {
                    Description = field.Text;
                }

                if (field.Field == "InputMask")
                {
                    InputMask = field.Text;
                }
            }
        }
    }
}