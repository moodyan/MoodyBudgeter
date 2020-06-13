using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Models.User.Search
{
    public class UserSearch
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string SearchFieldValue { get; set; }
    }
}
