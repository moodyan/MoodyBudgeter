using MoodyBudgeter.Models.Grid.Filters;
using System.Collections.Generic;

namespace MoodyBudgeter.Models.Grid
{
    public class GridRequest
    {
        public int PageSize { get; set; }
        public int PageOffset { get; set; }
        public List<string> SortExpressions { get; set; }
        public List<NumberFilter> NumberFilters { get; set; }
        public List<PassedIdList> PassedIdLists { get; set; }
        public List<StringFilter> StringFilters { get; set; }
        public List<DateRangeFilter> DateRangeFilters { get; set; }
        public List<BooleanFilter> BooleanFilters { get; set; }
    }
}
