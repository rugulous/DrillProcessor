using System.Collections;
using System.Reflection;

namespace DrillProcessor
{
    [Serializable]
    public class DrillExtractionException : Exception
    {
        public DrillExtractionException()
        {
        }

        public DrillExtractionException(string? message) : base(message)
        {
        }

        public DrillExtractionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DrillExtractionException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
