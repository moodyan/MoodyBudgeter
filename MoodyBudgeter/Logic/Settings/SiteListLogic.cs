using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Repositories.Settings;
using MoodyBudgeter.Utility.Cache;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Settings
{
    public class SiteListLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public SiteListLogic(IBudgeterCache cache, ContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<List<SiteList>> GetLists(bool isAdmin)
        {
            var siteListCache = new SiteListCache(Cache);

            var cacheResult = await siteListCache.GetSiteListsFromCache(isAdmin);

            if (cacheResult != null)
            {
                return cacheResult;
            }

            List<SiteList> listOfLists;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                var listOfListsRequest = repo.GetAll(isAdmin);

                listOfLists = await listOfListsRequest.ToListAsync();
            }

            if (listOfLists != null)
            {
                await siteListCache.AddSiteListToCache(listOfLists, isAdmin);
            }

            return listOfLists;
        }

        public async Task<Page<SiteList>> GetGrid(GridRequest gridRequest, bool isAdmin)
        {
            var data = new Page<SiteList>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                var query = repo.GetAll(isAdmin);

                var dataGridLogic = new DataGridLogic<SiteList>(gridRequest, query);

                data.Records = await dataGridLogic.GetResults();
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }

        public async Task<List<SiteList>> GetLists(bool isAdmin, string name)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                var foundListQuery = repo.GetAll(isAdmin).Where(x => x.Name.ToUpper() == name.ToUpper());

                return await foundListQuery.ToListAsync();
            }
        }

        public async Task<SiteList> GetListById(int id, bool isAdmin)
        {
            SiteList foundList;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                var foundListQuery = repo.GetAll(isAdmin).Where(x => x.SiteListId == id);

                foundList = await foundListQuery.FirstOrDefaultAsync();
            }

            return foundList;
        }

        public async Task<SiteList> CreateSiteList(SiteList list)
        {
            var existingList = await GetLists(true, list.Name);

            if (existingList != null && existingList.Count > 0)
            {
                throw new CallerException("List name already exists");
            }

            SiteList createdList;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                createdList = await repo.Create(list);
            }

            var siteListCache = new SiteListCache(Cache);

            await siteListCache.InvalidateSiteListCache();

            return createdList;
        }

        public async Task<SiteList> UpdateList(SiteList list)
        {
            var existingList = await GetListById(list.SiteListId, true);

            if (existingList == null)
            {
                throw new CallerException("Cannot find SiteList to update");
            }

            existingList.Name = list.Name;
            existingList.Visible = list.Visible;
            existingList.UpdatedBy = list.UpdatedBy;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                existingList = await repo.Update(existingList);
            }

            var siteListCache = new SiteListCache(Cache);

            await siteListCache.InvalidateSiteListCache();

            return existingList;
        }

        public async Task Delete(int listId)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListRepository(uow);

                await repo.Delete(listId);
            }

            var siteListCache = new SiteListCache(Cache);

            await siteListCache.InvalidateSiteListCache();
        }
    }
}
