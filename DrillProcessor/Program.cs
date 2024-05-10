using DrillProcessor;
using DrillProcessor.Interface;
using DrillProcessor.Model;

(string, int)[] movements = { ("P1", 190), ("P2", 140), ("P3", 95) }; //, "P4" };

IDrillExtractor extractor = new PywareDrillExtractor();

for(int i = 0; i < movements.Length; i++)
{
    List<RawPerformer> performers = extractor.Extract($"Drill/2024/{movements[i].Item1}.pdf");
    DotExporter.Export(performers, $"C:\\xampp\\htdocs\\Dr.Ill\\drill\\{movements[i].Item1}.drill", movements[i].Item2);
}
