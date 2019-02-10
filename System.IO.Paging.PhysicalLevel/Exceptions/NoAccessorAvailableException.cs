using System.Runtime.Serialization;

namespace System.IO.Paging.PhysicalLevel.Exceptions
{
    [Serializable]
    internal class NoAccessorAvailableException : Exception
    {
        public NoAccessorAvailableException()
        {
        }

        public NoAccessorAvailableException(string message) : base(message)
        {
        }

        public NoAccessorAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoAccessorAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}