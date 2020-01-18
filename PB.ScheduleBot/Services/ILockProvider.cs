using System.Threading;

namespace PB.ScheduleBot.Services
{
    public interface ILockProvider
    {
        Semaphore GetLockFor(string id);
    }
}