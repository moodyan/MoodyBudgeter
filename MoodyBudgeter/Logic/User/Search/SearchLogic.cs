using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.User.Profile;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User.Profile;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Search
{
    public class SearchLogic
    {
        //private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public SearchLogic(ContextWrapper context)
        {
            Context = context;
        }

        public async Task<Page<UserSearchResponse>> Search(UserSearch search)
        {
            //var searchCache = new SearchCache(Cache, PortalId);

            //var response = await searchCache.GetSearchResponseFromCache(search);
            Page<UserSearchResponse> response = null;

            if (response != null)
            {
                return response;
            }

            if (search.SearchUsername)
            {
                response = await SearchByUsername(search);
            }
            else
            {
                if (!search.ProfilePropertyId.HasValue && string.IsNullOrWhiteSpace(search.ProfilePropertyName))
                {
                    throw new CallerException("Must specify a ProfilePropertyId or ProfilePropertyName");
                }

                response = await SearchByProfileProperty(search);
            }

            //await searchCache.AddSearchResponseToCache(response, search);

            return response;
        }

        private async Task<Page<UserSearchResponse>> SearchByUsername(UserSearch search)
        {
            List<UserSearchResponse> records;
            var response = new Page<UserSearchResponse>();

            using (var uow = new UnitOfWork(Context))
            {
                var userRepo = new UserRepository(uow);
                IQueryable<UserSearchResponse> query = userRepo.SearchByUsername(search.SearchText, search.Operator, search.IncludeAvatar);

                response.TotalRecordCount = await query.CountAsync();

                var sortField = !string.IsNullOrWhiteSpace(search.SortField) ? search.SortField.Trim().ToLower() : "searchfieldvalue";
                response.SortExpression = GetSortExpression(sortField, search.SortAscending);

                query = query.OrderBy(response.SortExpression);

                records = await query.Skip(search.PageSize * search.PageOffset).Take(search.PageSize).ToListAsync();
            }

            response.PageSize = search.PageSize;
            response.PageOffset = search.PageOffset;

            response.Records = records;

            return response;
        }

        private async Task<Page<UserSearchResponse>> SearchByProfileProperty(UserSearch search)
        {
            List<UserSearchResponse> records;
            var response = new Page<UserSearchResponse>();

            var property = await ValidateProfilePropertyForSearch(search);

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserProfilePropertyRepository(uow);

                var query = repo.SearchByPropertyValue(search.SearchText, property.ProfilePropertyId, search.Operator, search.IncludeAvatar);

                response.TotalRecordCount = await query.CountAsync();

                var sortField = !string.IsNullOrWhiteSpace(search.SortField) ? search.SortField.Trim().ToLower() : "searchfieldvalue";
                response.SortExpression = GetSortExpression(sortField, search.SortAscending);

                query = query.OrderBy(response.SortExpression);

                records = await query.Skip(search.PageSize * search.PageOffset).Take(search.PageSize).ToListAsync();
            }

            response.PageSize = search.PageSize;
            response.PageOffset = search.PageOffset;

            response.Records = records;

            return response;
        }

        private async Task<ProfileProperty> ValidateProfilePropertyForSearch(UserSearch search)
        {
            var profilePropertyLogic = new ProfilePropertyLogic(Context);

            var property = search.ProfilePropertyId.HasValue ?
                await profilePropertyLogic.GetProfileProperty(search.ProfilePropertyId.Value, search.IsAdmin) :
                await profilePropertyLogic.GetProfileProperty(search.ProfilePropertyName, search.IsAdmin);

            if (property == null)
            {
                throw new CallerException("Specified profile property does not exist, or you do not have sufficient security privileges to access it");
            }

            // Non admins can only search Public profile properties.
            if (property.Visibility != ProfilePropertyVisibility.Public && !search.IsAdmin)
            {
                throw new CallerException("Specified profile property is not public, and therefore not searchable");
            }

            return property;
        }

        private string GetSortExpression(string field, bool ascending)
        {
            string sortExpression;

            switch (field)
            {
                case "displayname":
                    sortExpression = "DisplayName";
                    break;
                case "userid":
                    sortExpression = "UserId";
                    break;
                case "subaccountid":
                    sortExpression = "SubAccountId";
                    break;
                case "searchfieldvalue":
                    sortExpression = "SearchFieldValue";
                    break;
                default:
                    throw new CallerException("A valid Sort Field was not specified");
            }

            return ascending ? sortExpression + " ASC" : sortExpression + " DESC";
        }
    }
}