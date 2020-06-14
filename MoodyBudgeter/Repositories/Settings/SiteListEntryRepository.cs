using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.Settings;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Settings
{
    public class SiteListEntryRepository : Repository<SiteListEntry>
    {
        private readonly UnitOfWork Uow;

        public SiteListEntryRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<SiteListEntry> Find(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.SiteListEntry
                                  where r.SiteListEntryId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new ArgumentException("There is no SiteListEntry with Id " + id + ".");
            }

            return Translate(dbRecord);
        }

        public override IQueryable<SiteListEntry> GetAll()
        {
            return (from r in Uow.DbContext.SiteListEntry
                    select new SiteListEntry
                    {
                        SiteListEntryId = r.SiteListEntryId,
                        Value = r.Value,
                        DateCreated = r.DateCreated,
                        UpdatedBy = r.UpdatedBy,
                        DateUpdated = r.DateUpdated,
                        SiteListId = r.SiteListId
                    });
        }

        public IQueryable<SiteListEntry> GetAll(bool isAdmin)
        {
            return (from r in Uow.DbContext.SiteListEntry
                    where (r.SiteList.Visible || isAdmin)
                    select new SiteListEntry
                    {
                        SiteListEntryId = r.SiteListEntryId,
                        Value = r.Value,
                        DateCreated = r.DateCreated,
                        UpdatedBy = r.UpdatedBy,
                        DateUpdated = r.DateUpdated,
                        SiteListId = r.SiteListId
                    });
        }

        public async override Task<SiteListEntry> Create(SiteListEntry entity)
        {
            var dbRecord = new DbSiteListEntry
            {
                Value = entity.Value,
                SiteListId = entity.SiteListId,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                UpdatedBy = entity.UpdatedBy
            };

            Uow.DbContext.SiteListEntry.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<SiteListEntry> Update(SiteListEntry entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = (from r in Uow.DbContext.SiteListEntry
                            where entity.SiteListEntryId == r.SiteListEntryId
                            select r).FirstOrDefault();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("No List Entry found.");
            }

            // Update the database record.
            dbRecord.Value = entity.Value;
            dbRecord.UpdatedBy = entity.UpdatedBy;
            dbRecord.DateUpdated = DateTime.UtcNow;


            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.SiteListEntry
                                  where r.SiteListEntryId == id
                                  select r).FirstOrDefaultAsync();

            Uow.DbContext.SiteListEntry.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private SiteListEntry Translate(DbSiteListEntry dbEntry)
        {
            SiteListEntry newList = new SiteListEntry
            {
                SiteListEntryId = dbEntry.SiteListEntryId,
                SiteListId = dbEntry.SiteListId,
                Value = dbEntry.Value,
                DateCreated = dbEntry.DateCreated,
                DateUpdated = dbEntry.DateUpdated,
                UpdatedBy = dbEntry.UpdatedBy
            };

            return newList;
        }
    }
}
