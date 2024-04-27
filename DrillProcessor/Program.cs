using DrillProcessor;
using System.Drawing;

string[] movements = { "P1", "P2", "P3", "P4" };

for(int i = 0; i < movements.Length; i++)
{
    List<RawPerformer> performers = DrillExtractor.Extract($"Drill/2023/{movements[i]}.pdf");
    DotExporter.Export(performers, $"C:\\temp\\drill\\{movements[i]}.drill");
}
