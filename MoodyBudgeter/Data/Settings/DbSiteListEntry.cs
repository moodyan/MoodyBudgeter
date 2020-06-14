using System;
using System.ComponentModel.DataAnnotations;

namespace MoodyBudgeter.Data.Settings
{
    public class DbSiteListEntry
    {
        [Key]
        public int SiteListEntryId { get; set; }
        public int SiteListId { get; set; }
        public string Value { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int UpdatedBy { get; set; }

        public DbSiteList SiteList { get; set; }
    }
}
