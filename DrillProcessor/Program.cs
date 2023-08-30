using DrillProcessor;
using System.Drawing;

List<Performer> opener = DrillExtractor.Extract("Drill/P1.pdf");

List<Performer> performers = DrillExtractor.Extract("Drill/P4.pdf", 
    DrillExtractor.Extract("Drill/P3.pdf", 
        DrillExtractor.Extract("Drill/P2.pdf", 
            opener
        )
    )
);

DotPlotter plotter = new(100);
Performer matt = performers.FirstOrDefault(p => p.Label == "S2");
Bitmap? dots = plotter.PlotDots(matt);
dots?.Save("C:\\temp\\drill\\Matt.bmp");
//foreach (Performer performer in performers)
//{
//    using Bitmap? bmp = plotter.PlotDots(performer);
//    if (OperatingSystem.IsWindows())
//    {
//        bmp?.Save($"C:\\temp\\drill\\{performer.Label}.bmp");
//    }
//}

DotExporter.Export(opener.First(p => p.Label == "S2"), "C:\\temp\\drill\\matt.txt");