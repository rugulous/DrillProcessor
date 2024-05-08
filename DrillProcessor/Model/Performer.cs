namespace DrillProcessor.Model
{
    internal class Performer
    {
        public string Name { get; set; }
        public string? Symbol { get; set; }
        public List<Dot> Dots { get; set; }

        public Performer(string name, string? symbol)
        {
            Name = name;
            Symbol = symbol;
            Dots = new();
        }
    }
}
