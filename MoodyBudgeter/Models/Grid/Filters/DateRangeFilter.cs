using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.Grid.Filters
{
    public class DateRangeFilter
    {
        public string FieldName { get; set; }
        public DateOperator Operator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public enum DateOperator
    {
        EqualTo,
        GreaterThan,
        LessThan
    }
}
