using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoodyBudgeter.Data.Settings
{
    public class DbSiteList
    {
        [Key]
        public int SiteListId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int UpdatedBy { get; set; }
        public bool Visible { get; set; }

        public ICollection<DbSiteListEntry> SiteListEntry { get; set; }
    }
}
