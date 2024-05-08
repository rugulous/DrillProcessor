namespace DrillProcessor.Model
{
    internal class PerformanceSet
    {
        public string Identifier { get; set; } = "";
        public string? RehearsalMark { get; set; }
        public int? CountsFromLastSet { get; set; }
        public int? CountsToNextSet { get; set; }
    }
}
