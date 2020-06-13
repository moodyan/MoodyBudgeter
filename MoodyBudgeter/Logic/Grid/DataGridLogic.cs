using MoodyBudgeter.Models.Grid;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Grid
{
    public class DataGridLogic<T>
    {
        public DataGridLogic(GridRequest gridRequest, IQueryable<T> query){ }

        public int PageSize { get; set; }
        public int PageOffset { get; set; }
        public int TotalRecordCount { get; set; }
        public string SortExpression { get; set; }

        public Task<List<T>> GetResults()
        {
            throw new System.NotSupportedException();
        }
    }
}
