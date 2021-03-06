﻿using System;

namespace MoodyBudgeter.Data.User
{
    public class UserRoles
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? IsTrialUsed { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public int Status { get; set; }
        public bool IsOwner { get; set; }

        public Roles Role { get; set; }
        public Users User { get; set; }
    }
}
