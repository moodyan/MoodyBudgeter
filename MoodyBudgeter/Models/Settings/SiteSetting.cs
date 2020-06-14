using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.Settings
{
    public class SiteSetting
    {
        public int SiteSettingId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}
