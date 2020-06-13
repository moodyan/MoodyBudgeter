﻿using System;
using System.Collections.Generic;

namespace MoodyBudgeter.Data.User
{
    public class Users
    {
        public Users()
        {
            UserProfile = new HashSet<UserProfile>();
            UserRoles = new HashSet<UserRoles>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsSuperUser { get; set; }
        public int? AffiliateId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool UpdatePassword { get; set; }
        public string LastIpaddress { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public Guid? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        public string LowerEmail { get; set; }
        
        public ICollection<UserProfile> UserProfile { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
