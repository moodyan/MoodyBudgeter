using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.Settings;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Settings;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Settings
{
    public class SiteListRepository : Repository<SiteList>
    {
        private readonly UnitOfWork Uow;

        public SiteListRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public async override Task<SiteList> Find(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.SiteList
                                  where r.SiteListId == id
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("There is no SiteList with id: " + id);
            }

            return Translate(dbRecord);
        }

        public override IQueryable<SiteList> GetAll()
        {
            return from r in Uow.DbContext.SiteList
                   select new SiteList
                   {
                       SiteListId = r.SiteListId,
                       Name = r.Name,
                       DateCreated = r.DateCreated,
                       DateUpdated = r.DateUpdated,
                       UpdatedBy = r.UpdatedBy,
                       Visible = r.Visible
                   };
        }

        public IQueryable<SiteList> GetAll(bool isAdmin)
        {
            return from r in Uow.DbContext.SiteList
                   where (r.Visible || isAdmin)
                   select new SiteList
                   {
                       SiteListId = r.SiteListId,
                       Name = r.Name,
                       DateCreated = r.DateCreated,
                       DateUpdated = r.DateUpdated,
                       UpdatedBy = r.UpdatedBy,
                       Visible = r.Visible
                   };
        }

        public async override Task<SiteList> Create(SiteList entity)
        {
            var dbRecord = new DbSiteList
            {
                Name = entity.Name,
                Visible = entity.Visible,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                UpdatedBy = entity.UpdatedBy
            };

            Uow.DbContext.SiteList.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<SiteList> Update(SiteList entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = (from r in Uow.DbContext.SiteList
                            where entity.SiteListId == r.SiteListId
                            select r).FirstOrDefault();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("No list found.");
            }

            // Update the database record.
            dbRecord.Name = entity.Name;
            dbRecord.UpdatedBy = entity.UpdatedBy;
            dbRecord.DateUpdated = DateTime.UtcNow;
            dbRecord.Visible = entity.Visible;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.SiteList
                                  where r.SiteListId == id
                                  select r).FirstOrDefaultAsync();

            Uow.DbContext.SiteList.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private SiteList Translate(DbSiteList dbEntry)
        {
            return new SiteList
            {
                SiteListId = dbEntry.SiteListId,
                Name = dbEntry.Name,
                DateCreated = dbEntry.DateCreated,
                DateUpdated = dbEntry.DateUpdated,
                UpdatedBy = dbEntry.UpdatedBy,
                Visible = dbEntry.Visible
            };
        }
    }
}
