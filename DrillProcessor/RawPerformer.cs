namespace DrillProcessor
{
    public class RawPerformer
    {
        public string Name { get; set; }
        public string? Label { get; set; }
        public List<DrillSet> Sets { get; set; }

        public RawPerformer(string name, string? label = null)
        {
            Name = name;
            Label = label;
            Sets = new();
        }
    }
}
