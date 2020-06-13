using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Data.User
{
    public partial class RoleGroups
    {
        public RoleGroups()
        {
            Roles = new HashSet<Roles>();
        }

        public int RoleGroupId { get; set; }
        public int PortalId { get; set; }
        public string RoleGroupName { get; set; }
        public string Description { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }

        public ICollection<Roles> Roles { get; set; }
    }
}
