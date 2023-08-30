using DrillProcessor;
using System.Drawing;

List<Performer> performers = DrillExtractor.Extract("Mvt 2_DotSheets.pdf");

DotPlotter plotter = new(100);
foreach (Performer performer in performers)
{
    using Bitmap? bmp = plotter.PlotDots(performer);
    if (OperatingSystem.IsWindows())
    {
        bmp?.Save($"C:\\temp\\drill\\{performer.Label}.bmp");
    }
}
