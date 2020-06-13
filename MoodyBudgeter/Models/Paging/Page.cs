using System.Collections.Generic;

namespace MoodyBudgeter.Models.Paging
{
    public class Page<T> where T : class
    {
        public List<T> Records { get; set; }
        public int PageSize { get; set; }
        public int PageOffset { get; set; }
        public int TotalRecordCount { get; set; }
        public string SortExpression { get; set; }
    }
}
