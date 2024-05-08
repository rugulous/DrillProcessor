using DrillProcessor.Interface;
using DrillProcessor.Model;

namespace DrillProcessor.Extractor
{
    internal class DrillExtractor
    {
        private IExtractionStrategy Strategy { get; }

        public DrillExtractor(IExtractionStrategy strategy)
        {
            Strategy = strategy;
        }

        public List<RawPerformer> Extract(string text)
        {
            string[] performerChunks = Strategy.PrepareChunks(text);

            List<RawPerformer> performers = new();
            foreach (string chunk in performerChunks)
            {
                string[] lines = chunk.Split('\n');
                performers.Add(Strategy.ExtractPerformer(lines));
            }

            return performers;
        }
    }
}
