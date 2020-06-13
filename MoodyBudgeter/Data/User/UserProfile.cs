using System;

namespace MoodyBudgeter.Data.User
{
    public partial class UserProfile
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public int? SeatId { get; set; }
        public int PropertyDefinitionId { get; set; }
        public string PropertyValue { get; set; }
        public string PropertyText { get; set; }
        public int Visibility { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtendedVisibility { get; set; }
        public ProfilePropertyDefinition PropertyDefinition { get; set; }
        public Users User { get; set; }
    }
}
