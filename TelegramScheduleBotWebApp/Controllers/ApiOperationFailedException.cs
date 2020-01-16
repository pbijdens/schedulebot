using System;
using System.Runtime.Serialization;

namespace TelegramScheduleBotWebApp.Controllers
{
    [Serializable]
    internal class ApiOperationFailedException : Exception
    {
        public ApiOperationFailedException()
        {
        }

        public ApiOperationFailedException(string message) : base(message)
        {
        }

        public ApiOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApiOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}