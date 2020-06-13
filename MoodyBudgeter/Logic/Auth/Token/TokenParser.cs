using MoodyBudgeter.Models.User.Roles;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace MoodyBudgeter.Logic.Auth.Token
{
    public class TokenParser
    {
        public int UserId { get; set; }
        public int PortalId { get; set; }
        public List<SecurityRole> SecurityRoles { get; set; }

        public TokenParser(string strToken)
        {
            JwtSecurityToken token;

            try
            {
                token = new JwtSecurityToken(strToken);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Could not parse Jwt token.", ex);
            }

            SecurityRoles = new List<SecurityRole>();

            foreach (var claim in token.Claims)
            {
                if (claim.Type == "uid")
                {
                    UserId = int.Parse(claim.Value);
                }

                if (claim.Type == "pid")
                {
                    PortalId = int.Parse(claim.Value);
                }

                if (claim.Type == "rol" && Enum.TryParse(claim.Value, out SecurityRole role))
                {
                    SecurityRoles.Add(role);
                }
            }
        }
    }
}
