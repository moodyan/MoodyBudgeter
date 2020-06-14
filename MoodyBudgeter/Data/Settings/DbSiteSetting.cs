using System;

namespace MoodyBudgeter.Data.Settings
{
    public class DbSiteSetting
    {
        public int SiteSettingId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public string CultureCode { get; set; }
    }
}
