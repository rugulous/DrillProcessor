using DrillProcessor;
using DrillProcessor.Interface;
using System.Drawing;

string[] movements = { "P2" }; //, "P3", "P4" };

IDrillExtractor extractor = new PywareDrillExtractor();

for(int i = 0; i < movements.Length; i++)
{
    List<RawPerformer> performers = extractor.Extract($"Drill/2024/{movements[i]}.pdf");
    DotExporter.Export(performers, $"C:\\xampp\\htdocs\\Dr.Ill\\drill\\{movements[i]}.drill");
}
