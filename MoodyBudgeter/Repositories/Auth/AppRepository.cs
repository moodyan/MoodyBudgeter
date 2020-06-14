using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Data.Auth;
using MoodyBudgeter.Models.Auth.App;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class AppRepository : Repository<AppModel>
    {
        private readonly UnitOfWork Uow;

        public AppRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public override Task<AppModel> Find(int id)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<AppModel> GetAll()
        {
            return (from r in Uow.DbContext.App
                    select new AppModel
                    {
                        ClientId = r.ClientId,
                        ClientSecret = r.ClientSecret,
                        Name = r.Name,
                        AllowImplicit = r.AllowImplicit,
                        AllowAuthCode = r.AllowAuthCode,
                        AllowClientCredentials = r.AllowClientCredentials,
                        RedirectUri = r.RedirectUri,
                        UserId = r.UserId,
                        DateCreated = r.DateCreated,
                        DateUpdated = r.DateUpdated,
                        UpdateBy = r.UpdatedBy,
                    });
        }

        public async override Task<AppModel> Create(AppModel entity)
        {
            var dbRecord = new AuthApp
            {
                ClientId = entity.ClientId,
                ClientSecret = entity.ClientSecret,
                Name = entity.Name,
                AllowImplicit = entity.AllowImplicit,
                AllowAuthCode = entity.AllowAuthCode,
                AllowClientCredentials = entity.AllowClientCredentials,
                RedirectUri = entity.RedirectUri,
                UserId = entity.UserId,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                UpdatedBy = entity.UpdateBy,
            };

            Uow.DbContext.App.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<AppModel> Update(AppModel entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = (from r in Uow.DbContext.App
                            where entity.ClientId == r.ClientId
                            select r).FirstOrDefault();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no App with that Id.");
            }

            // Update the database record.
            dbRecord.Name = entity.Name;
            dbRecord.AllowImplicit = entity.AllowImplicit;
            dbRecord.AllowAuthCode = entity.AllowAuthCode;
            dbRecord.AllowClientCredentials = entity.AllowClientCredentials;
            dbRecord.RedirectUri = entity.RedirectUri;
            dbRecord.UserId = entity.UserId;
            dbRecord.DateUpdated = DateTime.UtcNow;
            dbRecord.UpdatedBy = entity.UpdateBy;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public override Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(string clientId)
        {
            var dbRecord = await (from r in Uow.DbContext.App
                                  where r.ClientId == clientId
                                  select r).FirstOrDefaultAsync();

            if (dbRecord == null)
            {
                throw new CallerException("App not found");
            }

            Uow.DbContext.App.Remove(dbRecord);
            await Uow.SaveChanges();
        }

        private AppModel Translate(AuthApp dbRecord)
        {
            return new AppModel
            {
                ClientId = dbRecord.ClientId,
                ClientSecret = dbRecord.ClientSecret,
                Name = dbRecord.Name,
                AllowImplicit = dbRecord.AllowImplicit,
                AllowAuthCode = dbRecord.AllowAuthCode,
                AllowClientCredentials = dbRecord.AllowClientCredentials,
                RedirectUri = dbRecord.RedirectUri,
                UserId = dbRecord.UserId,
                DateCreated = dbRecord.DateCreated,
                DateUpdated = dbRecord.DateUpdated,
                UpdateBy = dbRecord.UpdatedBy,
            };
        }
    }
}
