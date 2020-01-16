using System;
using System.Runtime.Serialization;

namespace PB.ScheduleBot.API
{
    [Serializable]
    public class TelegramAPIException : Exception
    {
        public TelegramAPIException()
        {
        }

        public TelegramAPIException(string message) : base(message)
        {
        }

        public TelegramAPIException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TelegramAPIException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}