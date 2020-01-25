using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot
{
    public class TelegramApiResult<T>
    {
        public bool ok { get; set; }
        public T result { get; set; }
    }
}
