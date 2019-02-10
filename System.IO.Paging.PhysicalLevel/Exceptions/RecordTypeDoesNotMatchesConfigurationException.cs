using System.Runtime.Serialization;

namespace System.IO.Paging.PhysicalLevel.Exceptions
{
    [Serializable]
    internal class RecordTypeDoesNotMatchesConfigurationException : Exception
    {
        public RecordTypeDoesNotMatchesConfigurationException()
        {
        }

        public RecordTypeDoesNotMatchesConfigurationException(string message) : base(message)
        {
        }

        public RecordTypeDoesNotMatchesConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RecordTypeDoesNotMatchesConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}