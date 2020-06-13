using MoodyBudgeter.Data.User;

namespace MoodyBudgeter.Repositories.User
{
    public class ContextWrapper
    {
        public UserContext Context { get; set; }
        public bool Injected { get; set; }

        public ContextWrapper() { }

        public ContextWrapper(UserContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
