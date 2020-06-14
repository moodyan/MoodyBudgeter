using MoodyBudgeter.Data.User;

namespace MoodyBudgeter.Repositories.User
{
    public class UserContextWrapper
    {
        public UserContext Context { get; set; }
        public bool Injected { get; set; }

        public UserContextWrapper() { }

        public UserContextWrapper(UserContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
