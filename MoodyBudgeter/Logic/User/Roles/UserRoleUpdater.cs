using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Cache;
using System;
using System.Threading.Tasks;

namespace MoodyBudgeter.Logic.User.Roles
{
    public class UserRoleUpdater
    {
        private readonly IBudgeterCache Cache;
        private readonly UserContextWrapper Context;

        public UserRoleUpdater(IBudgeterCache cache, UserContextWrapper context)
        {
            Cache = cache;
            Context = context;
        }

        public async Task<UserRole> AddRoleToUser(UserRole userRole)
        {
            await ValidateAddOrRemoveRole(userRole);

            UserRole existingUserRole = await new UserRoleLogic(Cache, Context).GetUserRole(userRole.RoleId, userRole.UserId);

            if (existingUserRole != null)
            {
                bool roleisExpired = existingUserRole.ExpiryDate == null ? false : existingUserRole.ExpiryDate < DateTime.UtcNow;

                userRole = await UpdateRole(existingUserRole, userRole);

                if (roleisExpired)
                {
                    //await SendRoleUpdatedEvent(userRole, true);
                }
            }
            else
            {
                userRole = await AddRole(userRole);

                //await SendRoleUpdatedEvent(userRole, true);
            }

            var userRoleCache = new UserRoleCache(Cache);

            await userRoleCache.InvalidateUserRoleCache(userRole.UserId);

            return userRole;
        }

        public async Task RemoveRoleFromUser(int userId, int roleId)
        {
            UserRole existingUserRole = await new UserRoleLogic(Cache, Context).GetUserRole(roleId, userId);

            await RemoveRoleFromUser(existingUserRole);
        }

        public async Task RemoveRoleFromUser(int userRoleId)
        {
            UserRole existingUserRole = await new UserRoleLogic(Cache, Context).GetUserRole(userRoleId, true);

            await RemoveRoleFromUser(existingUserRole);
        }

        private async Task RemoveRoleFromUser(UserRole userRole)
        {
            if (userRole == null)
            {
                return;
            }

            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                await repo.Delete(userRole.UserRoleId);
            }

            var userRoleCache = new UserRoleCache(Cache);

            await userRoleCache.InvalidateUserRoleCache(userRole.UserId);

            //await SendRoleUpdatedEvent(userRole, false);
        }

        private async Task<UserRole> UpdateRole(UserRole existingRole, UserRole role)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                existingRole.ExpiryDate = role.ExpiryDate;

                return await repo.Update(existingRole);
            }
        }

        private async Task<UserRole> AddRole(UserRole role)
        {
            using (var uow = new UnitOfWork(Context))
            {
                var repo = new UserRoleRepository(uow);

                return await repo.Create(role);
            }
        }

        private async Task ValidateAddOrRemoveRole(UserRole userRole)
        {
            if (userRole == null)
            {
                throw new CallerException("No Role data provided.");
            }

            if (userRole.UserId <= 0)
            {
                throw new CallerException("UserId is required.");
            }

            if (userRole.RoleId <= 0 && string.IsNullOrEmpty(userRole.RoleName))
            {
                throw new CallerException("A RoleName or RoleId is Required.");
            }

            var roleLogic = new RoleLogic(Cache, Context);

            if (userRole.RoleId <= 0)
            {
                var foundRole = await roleLogic.GetRoleFromName(userRole.RoleName);

                if (foundRole == null || foundRole.RoleId <= 0)
                {
                    throw new CallerException("Role could not be found");
                }

                userRole.RoleId = foundRole.RoleId;
            }
            else
            {
                var role = await roleLogic.GetRole(userRole.RoleId, true);

                if (role == null)
                {
                    throw new CallerException("Role could not found");
                }
            }
        }

        //private async Task SendRoleUpdatedEvent(UserRole userRole, bool add)
        //{
        //    var message = new UserRoleUpdated
        //    {
        //        UserRoleId = userRole.UserRoleId,
        //        UserId = userRole.UserId,
        //        RoleId = userRole.RoleId,
        //        Add = add,
        //        Level = Level
        //    };

        //    await QueueSender.SendMessage<UserRoleUpdated>(message);
        //}
    }
}
