using MoodyBudgeter.Models.Exceptions;
using MoodyBudgeter.Utility.Cache.Redis;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoodyBudgeter.Utility.Lock
{
    public class BudgeterLock : IBudgeterLock
    {
        private const int SECONDS_TO_EXPIRE = 20;
        private const int SECONDS_TO_WAIT = 40;
        private const int MILLISECONDS_TO_RETRY = 25;
        private const string LOCK_PREFIX = "MoodyBudgeter:";

        private readonly RedLockFactory Factory;

        public BudgeterLock()
        {
            var endPoints = new List<RedLockMultiplexer>
            {
                RedisConnection.Connection
            };

            Factory = RedLockFactory.Create(endPoints);
        }

        public async Task<IRedLock> Lock(string key)
        {
            var redlock = await Factory.CreateLockAsync(LOCK_PREFIX + key, new TimeSpan(0, 0, SECONDS_TO_EXPIRE), new TimeSpan(0, 0, SECONDS_TO_WAIT), new TimeSpan(0, 0, 0, 0, MILLISECONDS_TO_RETRY));

            if (redlock.IsAcquired == false)
            {
                throw new BudgeterException($"Code lock expired without being able to enter the critical section. Key: {redlock.Resource} LockId: {redlock.LockId}");
            }

            return redlock;
        }
    }
}
