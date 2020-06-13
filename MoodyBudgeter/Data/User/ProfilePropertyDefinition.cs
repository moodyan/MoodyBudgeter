using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Data.User
{
    public partial class ProfilePropertyDefinition
    {
        public ProfilePropertyDefinition()
        {
            UserProfile = new HashSet<UserProfile>();
        }

        public int PropertyDefinitionId { get; set; }
        public int? ModuleDefId { get; set; }
        public bool Deleted { get; set; }
        public int DataType { get; set; }
        public string DefaultValue { get; set; }
        public string PropertyCategory { get; set; }
        public string PropertyName { get; set; }
        public int Length { get; set; }
        public bool Required { get; set; }
        public string ValidationExpression { get; set; }
        public int ViewOrder { get; set; }
        public bool Visible { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public int? DefaultVisibility { get; set; }
        public bool ReadOnly { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        public string Label { get; set; }
        public string PcreRegex { get; set; }
        public string JsRegex { get; set; }
        public bool Unique { get; set; }
        public string InputMask { get; set; }
        public int? ListId { get; set; }
        public int DataTypeEnum { get; set; }

        public ICollection<UserProfile> UserProfile { get; set; }
    }
}