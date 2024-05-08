namespace DrillProcessor.Model
{
    public class DrillSet
    {
        public string Identifier { get; set; } = "";
        public string? RehearsalMark { get; set; }
        public int? CountsFromLastSet { get; set; }
        public int? CountsToNextSet { get; set; }
        public string RawCoordsX { get; set; } = "";
        public string RawCoordsY { get; set; } = "";
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
    }
}
