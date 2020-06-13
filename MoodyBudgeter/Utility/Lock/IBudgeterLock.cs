using RedLockNet;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Lock
{
    public interface IBudgeterLock
    {
        Task<IRedLock> Lock(string key);
    }
}
