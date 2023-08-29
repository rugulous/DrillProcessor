namespace DrillProcessor
{
    internal class Performer
    {
        public string Name { get; set; }
        public string? Label { get; set; }
        public List<DrillSet> Sets { get; set; }

        public Performer(string name, string? label = null)
        {
            Name = name;
            Label = label;
            Sets = new();
        }
    }
}
