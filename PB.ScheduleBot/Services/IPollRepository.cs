using PB.ScheduleBot.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Services
{
    public interface IPollRepository
    {
        Task<Poll> LoadAsync(string id);
        Task<Poll> SaveAsync(Poll poll);
    }
}
