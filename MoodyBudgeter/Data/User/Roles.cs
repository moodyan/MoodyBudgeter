using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Data.User
{
    public class Roles
    {
        public Roles()
        {
            UserRoles = new HashSet<UserRoles>();
        }

        public int RoleId { get; set; }
        public int PortalId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public decimal? ServiceFee { get; set; }
        public string BillingFrequency { get; set; }
        public int? TrialPeriod { get; set; }
        public string TrialFrequency { get; set; }
        public int? BillingPeriod { get; set; }
        public decimal? TrialFee { get; set; }
        public bool IsPublic { get; set; }
        public bool AutoAssignment { get; set; }
        public int? RoleGroupId { get; set; }
        public string Rsvpcode { get; set; }
        public string IconFile { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public int SecurityMode { get; set; }
        public bool IsSystemRole { get; set; }
        public int Ordinal { get; set; }

        public RoleGroups RoleGroup { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
