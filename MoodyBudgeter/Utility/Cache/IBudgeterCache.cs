using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Cache
{
    public interface IBudgeterCache
    {
        Task<T> Get<T>(string key);
        Task Insert(string key, object value, TimeSpan timeToLive);
        Task Remove(string key);
        Task ScanRedisAndRemovePrefix(string prefix);
    }
}
