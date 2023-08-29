namespace DrillProcessor
{
    internal class DrillSet
    {
        public string Identifier { get; set; }
        public string? RehearsalMark { get; set; }
        public int? CountsFromLastSet { get; set; }
        public int? CountsToNextSet { get; set; }
        public string rawCoordsX { get; set; }
        public string rawCoordsY { get; set; }
    }
}
