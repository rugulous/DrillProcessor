namespace DrillProcessor
{
    internal class Performance
    {
        public string Title { get; set; }
        public int Bpm { get; set; }
        public List<PerformanceSet> Sets { get; set; }
        public List<Performer> Performers { get; set; }

        public Performance(string title, int bpm)
        {
            Title = title;
            Bpm = bpm;
            Sets = new();
            Performers = new();
        }
    }
}
