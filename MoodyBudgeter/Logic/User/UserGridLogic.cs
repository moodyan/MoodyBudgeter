using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.User;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User
{
    public class UserGridLogic
    {
        private readonly UserContextWrapper Context;

        public UserGridLogic(UserContextWrapper context)
        {
            Context = context;
        }

        public async Task<Page<GridUser>> GetGrid(GridRequest gridRequest, int? profilePropertyId, string searchText, SearchOperator? searchOperator, bool includeDeleted)
        {
            var data = new Page<GridUser>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRepository(uow);

                var userQuery = repo.GetAllForGrid(profilePropertyId, searchText, searchOperator, includeDeleted);

                var dataGridLogic = new DataGridLogic<GridUser>(gridRequest, userQuery);

                data.Records = await dataGridLogic.GetResults();
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }
    }
}
