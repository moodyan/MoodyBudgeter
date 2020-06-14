using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Search
{
    public class SearchCache
    {
        private const int CACHE_TIME_IN_MINUTES = 5;
        private const string CACHE_KEY_PREFIX = "User:Search:";
        
        private readonly IBudgeterCache Cache;

        public SearchCache(IBudgeterCache cache)
        {
            Cache = cache;
        }

        internal async Task<Page<UserSearchResponse>> GetSearchResponseFromCache(UserSearch searchOptions)
        {
            var key = GetCacheKey(searchOptions);

            return await Cache.Get<Page<UserSearchResponse>>(key);
        }

        internal async Task AddSearchResponseToCache(Page<UserSearchResponse> response, UserSearch searchOptions)
        {
            var key = GetCacheKey(searchOptions);

            await Cache.Insert(key, response, new TimeSpan(0, CACHE_TIME_IN_MINUTES, 0));
        }

        private string GetCacheKey(UserSearch searchOptions)
        {
            return CACHE_KEY_PREFIX + "-SearchText_" + searchOptions.SearchText +
                   "-ProfilePropertyName_" + searchOptions.ProfilePropertyName + "-ProfilePropertyId_" +
                   searchOptions.ProfilePropertyId + "-SearchUsername_" + searchOptions.SearchUsername +
                   "-SearchOperator_" + searchOptions.Operator + "-PageSize_" + searchOptions.PageSize + "-PageOffset_" +
                   searchOptions.PageOffset + "-SortAscending_" + searchOptions.SortAscending + "-SortField_" +
                   searchOptions.SortField + "-IncludeAvatar_" + searchOptions.IncludeAvatar + "-IsAdmin_" + searchOptions.IsAdmin;
        }
    }
}
