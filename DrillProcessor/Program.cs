using DrillProcessor;
using DrillProcessor.Interface;
using System.Drawing;

string[] movements = { "P1" }; //, "P2", "P3", "P4" };

IDrillExtractor extractor = new EnvisionDrillExtractor();

for(int i = 0; i < movements.Length; i++)
{
    List<RawPerformer> performers = extractor.Extract($"Drill/2023/{movements[i]}.pdf");
    DotExporter.Export(performers, $"C:\\xampp\\htdocs\\Dr.Ill\\drill\\{movements[i]}.drill");
}
