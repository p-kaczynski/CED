using System;
using System.Runtime.Serialization;

namespace CED
{
    public class EventDispatcherException : Exception
    {
        public EventDispatcherException()
        {
        }

        public EventDispatcherException(string message) : base(message)
        {
        }

        public EventDispatcherException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}