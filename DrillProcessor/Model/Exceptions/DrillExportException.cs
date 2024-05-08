namespace DrillProcessor.Model.Exceptions
{
    [Serializable]
    public class DrillExportException : Exception
    {
        public DrillExportException()
        {
        }

        public DrillExportException(string? message) : base(message)
        {
        }

        public DrillExportException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DrillExportException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
