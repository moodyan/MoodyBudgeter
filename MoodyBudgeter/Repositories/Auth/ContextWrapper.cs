using MoodyBudgeter.Data.Auth;

namespace MoodyBudgeter.Repositories.Auth
{
    public class ContextWrapper
    {
        public AuthContext Context { get; set; }
        public bool Injected { get; set; }

        public ContextWrapper() { }

        public ContextWrapper(AuthContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
