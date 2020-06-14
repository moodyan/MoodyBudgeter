using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User.Search;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers.User
{
    [Route("user/v1/[controller]")]
    public class SearchController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public SearchController(IBudgeterCache cache)
        {
            Cache = cache;
            Context = new UserContextWrapper();
        }

        [HttpGet]
        [BudgeterAuthorize]
        public async Task<Page<UserSearchResponse>> Get(string searchText, string profilePropertyName = null,
            int? profilePropertyId = null, bool searchUsername = false, bool searchSubAccounts = false,
            SearchOperator searchOperator = SearchOperator.Equals, int pageSize = 5, int pageOffset = 0,
            string sortField = null, bool sortAscending = true, bool includeAvatar = false)
        {
            var searchLogic = new SearchLogic(Cache, Context);

            var search = new UserSearch
            {
                SearchText = searchText,
                ProfilePropertyName = profilePropertyName,
                ProfilePropertyId = profilePropertyId,
                SearchUsername = searchUsername,
                Operator = searchOperator,
                PageSize = pageSize,
                PageOffset = pageOffset,
                IsAdmin = IsAdmin,
                SortAscending = sortAscending,
                SortField = sortField,
                IncludeAvatar = includeAvatar
            };

            return await searchLogic.Search(search);
        }
    }
}
