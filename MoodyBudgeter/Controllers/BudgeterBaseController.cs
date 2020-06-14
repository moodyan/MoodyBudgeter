using Microsoft.AspNetCore.Mvc;
using MoodyBudgeter.Logic.Auth.Token;
using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Models.User.Roles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoodyBudgeter.Controllers
{
    public class BudgeterBaseController : Controller
    {
        private TokenParser _token;

        private TokenParser Token
        {
            get
            {
                if (_token == null && HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationValue))
                {
                    string token = authorizationValue.ToString().Replace("Bearer ", "");

                    _token = new TokenParser(token);
                }

                return _token;
            }
        }

        protected int UserId
        {
            get
            {
                var token = Token;

                if (token == null)
                {
                    return -1;
                }

                return token.UserId;
            }
        }

        protected List<SecurityRole> SecurityRoles
        {
            get
            {
                var token = Token;

                if (token == null)
                {
                    return new List<SecurityRole>();
                }

                return token.SecurityRoles;
            }
        }

        protected bool IsAdmin
        {
            get
            {
                if (SecurityRoles == null)
                {
                    return false;
                }

                return SecurityRoles.Contains(SecurityRole.Admin);
            }
        }

        protected bool IsSuperUser
        {
            get
            {
                return SecurityRoles.Contains(SecurityRole.SuperUser);
            }
        }

        /// <summary>
        /// Sees if the caller is allowed to use the passed userId. Default for security roles allowed to pass is Admin.
        /// </summary>
        protected void CheckIfPassedUserIDAllowed(int? passedUserId, List<SecurityRole> securityRoles = null)
        {
            if (passedUserId == null)
            {
                return;
            }

            if (UserId == passedUserId)
            {
                return;
            }

            var userSecurityRoles = Token?.SecurityRoles;

            if (securityRoles == null)
            {
                if (userSecurityRoles != null && userSecurityRoles.Contains(SecurityRole.Admin))
                {
                    return;
                }
            }
            else
            {
                if (userSecurityRoles != null && securityRoles.Any(c => userSecurityRoles.Contains(c)))
                {
                    return;
                }
            }

            throw new UnauthorizedAccessException("You do not have the security roles required to perform an action for another user. (UserID: " + passedUserId.Value + ")");
        }

        protected void CheckNullBody(object body)
        {
            if (body == null)
            {
                throw new CallerException("Body cannot be null");
            }
        }
    }
}
