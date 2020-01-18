using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PB.ScheduleBot.Services
{
    public class LockProvider : ILockProvider
    {
        private ConcurrentDictionary<string, Semaphore> idToLockMap = new ConcurrentDictionary<string, Semaphore>();

        public Semaphore GetLockFor(string id)
        {
            return idToLockMap.GetOrAdd(id, (id) => new Semaphore(1, 1));
        }
    }
}
