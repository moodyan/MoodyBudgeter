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
    public class SiteSettingRepository : Repository<SiteSetting>
    {
        private readonly UnitOfWork Uow;

        public SiteSettingRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public override Task<SiteSetting> Find(int id)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<SiteSetting> GetAll()
        {
            return (from r in Uow.DbContext.SiteSettings
                    select new SiteSetting
                    {
                        SiteSettingId = r.SiteSettingId,
                        SettingName = r.SettingName,
                        SettingValue = r.SettingValue,
                        LastModifiedByUserId = r.LastModifiedByUserId ?? -1,
                        DateCreated = r.CreatedOnDate ?? new DateTime(),
                        DateUpdated = r.LastModifiedOnDate ?? new DateTime()
                    });
        }

        public async override Task<SiteSetting> Create(SiteSetting entity)
        {
            var dbRecord = new DbSiteSetting
            {
                SettingName = entity.SettingName,
                SettingValue = entity.SettingValue,
                CreatedByUserId = entity.LastModifiedByUserId,
                CreatedOnDate = DateTime.UtcNow,
                LastModifiedByUserId = entity.LastModifiedByUserId,
                LastModifiedOnDate = DateTime.UtcNow,
                CultureCode = null
            };

            Uow.DbContext.SiteSettings.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<SiteSetting> Update(SiteSetting entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = (from r in Uow.DbContext.SiteSettings
                            where entity.SiteSettingId == r.SiteSettingId
                            select r).FirstOrDefault();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("No setting found.");
            }

            // Update the database record.
            dbRecord.SettingValue = entity.SettingValue;
            dbRecord.LastModifiedByUserId = entity.LastModifiedByUserId;
            dbRecord.LastModifiedOnDate = DateTime.UtcNow;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public async override Task Delete(int id)
        {
            var dbRecord = await (from r in Uow.DbContext.SiteSettings
                                  where r.SiteSettingId == id
                                  select r).FirstOrDefaultAsync();

            Uow.DbContext.SiteSettings.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private SiteSetting Translate(DbSiteSetting dbRecord)
        {
            return new SiteSetting
            {
                SiteSettingId = dbRecord.SiteSettingId,
                SettingName = dbRecord.SettingName,
                SettingValue = dbRecord.SettingValue,
                LastModifiedByUserId = dbRecord.LastModifiedByUserId ?? -1,
                DateCreated = dbRecord.CreatedOnDate ?? new DateTime(),
                DateUpdated = dbRecord.LastModifiedOnDate ?? new DateTime()
            };
        }
    }
}
