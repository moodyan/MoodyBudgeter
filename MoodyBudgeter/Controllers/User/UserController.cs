using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.User;
using MoodyBudgeter.Logic.User.Registration;
using MoodyBudgeter.Models.Grid;
using MoodyBudgeter.Models.Paging;
using MoodyBudgeter.Models.User;
using MoodyBudgeter.Models.User.Registration;
using MoodyBudgeter.Models.User.Roles;
using MoodyBudgeter.Models.User.Search;
using MoodyBudgeter.Repositories.Auth;
using MoodyBudgeter.Repositories.User;
using MoodyBudgeter.Utility.Auth;
using MoodyBudgeter.Utility.Cache;
using MoodyBudgeter.Utility.Clients.Settings;
using MoodyBudgeter.Utility.Lock;
using System.Threading.Tasks;

namespace MoodyBudgeter.Controllers
{
    [Route("user/v1/[controller]")]
    public class UserController : BudgeterBaseController
    {
        private readonly IBudgeterCache Cache;
        private readonly IBudgeterLock BudgeterLock;
        private readonly ISettingRequester SettingRequester;
        private readonly UserContextWrapper UserContext;
        private readonly AuthContextWrapper AuthContext;

        public UserController(IBudgeterCache cache, IBudgeterLock budgeterLock, ISettingRequester settingRequester)
        {
            Cache = cache;
            BudgeterLock = budgeterLock;
            SettingRequester = settingRequester;
            UserContext = new UserContextWrapper();
            AuthContext = new AuthContextWrapper();
        }

        [HttpGet]
        [BudgeterAuthorize]
        public async Task<BudgetUser> Get()
        {
            var userLogic = new UserLogic(Cache, UserContext);

            return await userLogic.GetUserWithRelated(UserId, IsAdmin);
        }

        [HttpGet, Route("{id}")]
        [BudgeterAuthorize]
        public async Task<BudgetUser> Get(int id)
        {
            CheckIfPassedUserIDAllowed(id);

            var userLogic = new UserLogic(Cache, UserContext);

            return await userLogic.GetUserWithRelated(id, IsAdmin);
        }

        [HttpPost]
        public async Task<RegistrationRequest> Post([FromBody]RegistrationRequest registrationRequest)
        {
            CheckNullBody(registrationRequest);

            var registrationLogic = new RegistrationLogic(Cache, BudgeterLock, AuthContext, UserContext, SettingRequester, IsAdmin);

            return await registrationLogic.RegisterUser(registrationRequest);
        }

        [HttpPost, Route("grid")]
        [BudgeterAuthorize((int)SecurityRole.Admin)]
        public async Task<Page<GridUser>> Grid([FromBody] GridRequest gridRequest, int? profilePropertyId = null,
            string searchText = null, SearchOperator? searchOperator = null, bool includeDeleted = false)
        {
            CheckNullBody(gridRequest);

            var gridLogic = new UserGridLogic(UserContext);

            return await gridLogic.GetGrid(gridRequest, profilePropertyId, searchText, searchOperator, includeDeleted);
        }
    }
}
