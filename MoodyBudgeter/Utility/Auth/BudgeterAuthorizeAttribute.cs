using Microsoft.AspNetCore.Mvc.Filters;
using MoodyBudgeter.Logic.Auth.Token;
using MoodyBudgeter.Models.User.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Auth
{
    public class BudgeterAuthorizeAttribute : ActionFilterAttribute
    {
        public List<SecurityRole> SecurityRoles { get; set; }

        public BudgeterAuthorizeAttribute(params int[] values)
        {
            SecurityRoles = new List<SecurityRole>();

            foreach (var value in values)
            {
                SecurityRoles.Add((SecurityRole)value);
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationValue))
            {
                throw new AuthenticationException("Authorization is required for this request");
            }

            string strToken = authorizationValue.ToString();

            strToken = strToken.Replace("Bearer ", "");

            var token = new TokenParser(strToken);

            // No specific role required.
            if (SecurityRoles.Count == 0)
            {
                return;
            }

            // Always an 'OR' operation, a user needs to be in just one of the security roles.
            if (!token.SecurityRoles.Any(c => SecurityRoles.Contains(c)))
            {
                throw new UnauthorizedAccessException("You do not have the required security role to perform this function");
            }

            return;
        }
    }
}