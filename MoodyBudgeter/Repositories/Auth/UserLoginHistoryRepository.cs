using MoodyBudgeter.Data.Auth;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Utility.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Repositories.Auth
{
    public class UserLoginHistoryRepository : Repository<UserLoginHistory>
    {
        private readonly UnitOfWork Uow;

        public UserLoginHistoryRepository(UnitOfWork uow) : base()
        {
            Uow = uow;
        }

        public override Task<UserLoginHistory> Find(int id)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<UserLoginHistory> GetAll()
        {
            return (from r in Uow.DbContext.UserLoginHistory
                    select new UserLoginHistory
                    {
                        UserId = r.UserId,
                        LoginDate = r.LoginDate,
                        TokenType = (TokenType)r.TokenType,
                        Provider = r.Provider,
                        Audience = r.Audience
                    });
        }

        public async override Task<UserLoginHistory> Create(UserLoginHistory entity)
        {
            var dbRecord = new AuthUserLoginHistory
            {
                UserId = entity.UserId,
                LoginDate = entity.LoginDate,
                TokenType = (int)entity.TokenType,
                Provider = entity.Provider,
                Audience = entity.Audience
            };

            Uow.DbContext.UserLoginHistory.Add(dbRecord);

            await Uow.SaveChanges();

            return Translate(dbRecord);
        }

        public override Task<UserLoginHistory> Update(UserLoginHistory entity)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        private UserLoginHistory Translate(AuthUserLoginHistory dbRecord)
        {
            return new UserLoginHistory
            {
                UserId = dbRecord.UserId,
                LoginDate = dbRecord.LoginDate,
                TokenType = (TokenType)dbRecord.TokenType,
                Provider = dbRecord.Provider,
                Audience = dbRecord.Audience
            };
        }
    }
}
