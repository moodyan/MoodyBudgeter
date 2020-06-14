using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Repositories.Settings;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Settings
{
    public class SiteListEntryLogic
    {
        private readonly IBudgeterCache Cache;
        private readonly ContextWrapper Context;

        public SiteListEntryLogic(IBudgeterCache cache, ContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<List<SiteListEntry>> GetListEntries(int listId, bool isAdmin)
        {
            if (listId == 0)
            {
                throw new CallerException("Must Pass Valid ListId");
            }

            var siteListCache = new SiteListEntryCache(Cache);

            var cacheResult = await siteListCache.GetSiteEntriesFromCache(listId, isAdmin);

            if (cacheResult != null)
            {
                return cacheResult;
            }

            List<SiteListEntry> listOfEntries;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListEntryRepository(uow);

                listOfEntries = await repo.GetAll(isAdmin).Where(x => x.SiteListId == listId).ToListAsync();
            }

            if (listOfEntries != null)
            {
                await siteListCache.AddListEntryToCache(listId, listOfEntries, isAdmin);
            }

            return listOfEntries;
        }

        public async Task<SiteListEntry> GetListEntryById(int id, bool isAdmin)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListEntryRepository(uow);

                return await repo.GetAll(isAdmin).Where(x => x.SiteListEntryId == id).FirstOrDefaultAsync();
            }
        }

        public async Task<SiteListEntry> Create(SiteListEntry listEntry, bool isAdmin)
        {
            if (string.IsNullOrEmpty(listEntry.Value))
            {
                throw new CallerException("Value can't be empty");
            }

            if (listEntry.SiteListId < 1)
            {
                throw new CallerException("SiteListEntry must belong to a SiteList");
            }

            SiteListLogic siteListLogic = new SiteListLogic(Cache, Context);

            var list = siteListLogic.GetListById(listEntry.SiteListId, isAdmin);

            if (list == null)
            {
                throw new CallerException("List not found to add entry to");
            }

            var existingListEntries = await GetListEntries(listEntry.SiteListId, isAdmin);

            if (existingListEntries != null && existingListEntries.Select(c => c.Value.ToUpper()).Contains(listEntry.Value.ToUpper()))
            {
                throw new FriendlyException("CreateListEntry.ValueAlreadyExists", "ListEntry already exists");
            }

            var createdListEntry = await CreateListEntry(listEntry);

            SiteListEntryCache siteListCache = new SiteListEntryCache(Cache);

            await siteListCache.InvalidateListEntryCache(createdListEntry.SiteListId);

            return createdListEntry;
        }

        private async Task<SiteListEntry> CreateListEntry(SiteListEntry listEntry)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListEntryRepository(uow);

                return await repo.Create(listEntry);
            }
        }

        public async Task<SiteListEntry> UpdateListEntry(SiteListEntry listEntry)
        {
            if (string.IsNullOrEmpty(listEntry.Value))
            {
                throw new CallerException("Value field can't be empty");
            }

            var existingListEntry = await GetListEntryById(listEntry.SiteListEntryId, true);

            if (existingListEntry == null)
            {
                throw new CallerException("SiteListEntry not found");
            }

            existingListEntry.UpdatedBy = listEntry.UpdatedBy;
            existingListEntry.Value = listEntry.Value;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListEntryRepository(uow);

                existingListEntry = await repo.Update(existingListEntry);
            }

            var siteListCache = new SiteListEntryCache(Cache);

            await siteListCache.InvalidateListEntryCache(listEntry.SiteListId);

            return existingListEntry;
        }

        public async Task Delete(int listEntryId)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new SiteListEntryRepository(uow);

                await repo.Delete(listEntryId);
            }

            var siteListCache = new SiteListEntryCache(Cache);

            await siteListCache.InvalidateListEntryCache(listEntryId);
        }
    }
}
