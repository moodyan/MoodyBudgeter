using MoodyBudgeter.Data.Auth;

namespace MoodyBudgeter.Repositories.Auth
{
    public class AuthContextWrapper
    {
        public AuthContext Context { get; set; }
        public bool Injected { get; set; }

        public AuthContextWrapper() { }

        public AuthContextWrapper(AuthContext context)
        {
            Context = context;
            Injected = true;
        }
    }
}
