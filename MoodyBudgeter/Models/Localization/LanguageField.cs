using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.Localization
{
    public class LanguageField
    {
        public int LanguageFieldId { get; set; }
        public string LangCode { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Field { get; set; }
        public decimal? Value { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }
}
