using DrillProcessor.Model;

namespace DrillProcessor.Interface
{
    internal interface IExtractionStrategy
    {
        public string[] PrepareChunks(string fileContents);
        public RawPerformer ExtractPerformer(string[] lines);
    }
}
