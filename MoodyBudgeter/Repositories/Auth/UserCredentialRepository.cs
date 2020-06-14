using MoodyBudgeter.Data.Auth;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class UserCredentialRepository : Repository<UserCredential>
    {
        private readonly UnitOfWork Uow;

        public UserCredentialRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public override Task<UserCredential> Find(int id)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<UserCredential> GetAll()
        {
            return from r in Uow.DbContext.UserCredential
                   select new UserCredential
                   {
                       UserId = r.UserId,
                       Username = r.Username,
                       Password = r.Password,
                       PasswordSalt = r.PasswordSalt,
                       AttemptCount = r.AttemptCount,
                       FirstAttemptDate = r.FirstAttemptDate,
                       ResetToken = r.ResetToken,
                       ResetExpiration = r.ResetExpiration,
                       DateCreated = r.DateCreated,
                   };
        }
        
        public async override Task<UserCredential> Create(UserCredential entity)
        {
            var dbRecord = new AuthUserCredential
            {
                UserId = entity.UserId,
                Username = entity.Username,
                Password = entity.Password,
                PasswordSalt = entity.PasswordSalt,
                AttemptCount = entity.AttemptCount,
                FirstAttemptDate = entity.FirstAttemptDate,
                ResetToken = entity.ResetToken,
                ResetExpiration = entity.ResetExpiration,
                DateCreated = DateTime.UtcNow,
            };

            Uow.DbContext.UserCredential.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public async override Task<UserCredential> Update(UserCredential entity)
        {
            // Make sure we have valid data.
            if (entity == null)
            {
                throw new ArgumentNullException("Data cannot be null.");
            }

            // Get the existing record from the database.
            var dbRecord = (from r in Uow.DbContext.UserCredential
                            where entity.UserId == r.UserId
                            select r).FirstOrDefault();

            if (dbRecord == null)
            {
                // The record does not exist.
                throw new CallerException("There is no UserCredential with that Id.");
            }

            // Update the database record.
            dbRecord.UserId = entity.UserId;
            dbRecord.Username = entity.Username;
            dbRecord.Password = entity.Password;
            dbRecord.PasswordSalt = entity.PasswordSalt;
            dbRecord.AttemptCount = entity.AttemptCount;
            dbRecord.FirstAttemptDate = entity.FirstAttemptDate;
            dbRecord.ResetToken = entity.ResetToken;
            dbRecord.ResetExpiration = entity.ResetExpiration;

            await Uow.SaveChanges();

            // Return the response data.
            return Translate(dbRecord);
        }

        public override Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        private UserCredential Translate(AuthUserCredential dbRecord)
        {
            return new UserCredential
            {
                UserId = dbRecord.UserId,
                Username = dbRecord.Username,
                Password = dbRecord.Password,
                PasswordSalt = dbRecord.PasswordSalt,
                AttemptCount = dbRecord.AttemptCount,
                FirstAttemptDate = dbRecord.FirstAttemptDate,
                ResetToken = dbRecord.ResetToken,
                ResetExpiration = dbRecord.ResetExpiration,
                DateCreated = dbRecord.DateCreated,
            };
        }
    }
}
