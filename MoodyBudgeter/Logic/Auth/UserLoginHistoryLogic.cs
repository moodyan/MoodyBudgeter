using Microsoft.EntityFrameworkCore;
using MoodyBudgeter.Logic.Grid;
using MoodyBudgeter.Models.Auth;
using MoodyBudgeter.Models.Auth.Token;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Repositories.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.Auth
{
    public class UserLoginHistoryLogic
    {
        private readonly ContextWrapper Context;

        public UserLoginHistoryLogic(ContextWrapper context)
        {
            Context = context;
        }

        public async Task<UserLoginHistory> RecordUserLogin(int userId, TokenType type, string provider, string audience)
        {
            var userLoginHistory = new UserLoginHistory
            {
                UserId = userId,
                TokenType = type,
                Provider = provider,
                Audience = audience,
                LoginDate = DateTime.UtcNow
            };

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserLoginHistoryRepository(uow);

                return await repo.Create(userLoginHistory);
            }
        }

        public async Task<Page<UserLoginHistory>> GetUserLoginHistory(int userId, int pageSize, int pageOffset)
        {
            if (userId <= 0 || pageSize <= 0 || pageOffset < 0)
            {
                throw new CallerException("Invalid input");
            }

            int totalRecordCount;
            List<UserLoginHistory> userLogins;

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserLoginHistoryRepository(uow);

                var query = repo.GetAll().Where(c => c.UserId == userId).OrderByDescending(c => c.LoginDate);

                totalRecordCount = query.Count();

                userLogins = await query.Skip(pageSize * pageOffset).Take(pageSize).ToListAsync();
            }

            return new Page<UserLoginHistory>
            {
                PageOffset = pageOffset,
                PageSize = pageSize,
                TotalRecordCount = totalRecordCount,
                SortExpression = "LoginDate DESC",
                Records = userLogins
            };
        }

        public async Task<Page<UserLoginHistory>> GetGrid(GridRequest gridRequest, int userId)
        {
            var data = new Page<UserLoginHistory>();

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserLoginHistoryRepository(uow);

                var query = repo.GetAll().Where(u => u.UserId == userId);

                var dataGridLogic = new DataGridLogic<UserLoginHistory>(gridRequest, query);

                data.Records = await dataGridLogic.GetResults();
                data.PageSize = dataGridLogic.PageSize;
                data.PageOffset = dataGridLogic.PageOffset;
                data.TotalRecordCount = dataGridLogic.TotalRecordCount;
                data.SortExpression = dataGridLogic.SortExpression;
            }

            return data;
        }
    }
}
