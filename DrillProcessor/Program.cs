using DrillProcessor;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;
using System.Text.RegularExpressions;

string ExtractText(string file)
{
    StringBuilder drillBuilder = new();
    PdfDocument pdf = new(new PdfReader(file));
    for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
    {
        var page = pdf.GetPage(i);
        string text = PdfTextExtractor.GetTextFromPage(page);
        drillBuilder.AppendLine(text);
    }
    pdf.Close();

    return drillBuilder.ToString().Trim();
}

Performer ExtractPerformer(string chunk)
{
    string[] lines = chunk.Split('\n');
    Performer performer = new(lines[0].Trim(), lines[1].Replace("Label:", "").Trim());
    int tmpI = -1;
    decimal tmpD = -1;

    for (int i = 4; i < lines.Length; i++)
    {
        string[] parts = lines[i].Split(" ");

        DrillSet currentSet = new();
        currentSet.Identifier = parts[0];

        int whereDoesDrillStartFrom = -1;
        int whereDoesDrillEnd = -1;

        if (int.TryParse(parts[1], out tmpI))
        {
            currentSet.CountsFromLastSet = tmpI;
            whereDoesDrillStartFrom = 2;
        }
        else if (int.TryParse(parts[2], out tmpI) && parts[1] != "On")
        {
            //next part is the counts!
            currentSet.RehearsalMark = parts[1];
            currentSet.CountsFromLastSet = tmpI;
            whereDoesDrillStartFrom = 3;
        }
        else
        {
            whereDoesDrillStartFrom = 1;
        }

        if(i == lines.Length - 1)
        {
            //last line has no out / step size
            whereDoesDrillEnd = 0;
        } else
        {
            int spacesToTrim = -1;
            if (parts[parts.Length - 1] == "Halt")
            {
                spacesToTrim = 2;
            } else
            {
                spacesToTrim = 4; //24, 9 to 5
            }

            currentSet.CountsToNextSet = int.Parse(parts[parts.Length - spacesToTrim]);
            whereDoesDrillEnd = spacesToTrim;
        }

        string drillCoords = string.Join(" ", parts.Skip(whereDoesDrillStartFrom).SkipLast(whereDoesDrillEnd));
        (currentSet.rawCoordsX, currentSet.rawCoordsY) = ExtractCoords(drillCoords);

        performer.Sets.Add(currentSet);
    }

    return performer;
}

(string x, string y) ExtractCoords(string drillCoords)
{
    if(drillCoords == "You don't stop on this subset")
    {
        return (null, null);
    }

    Regex regex = new("(On 50)|((\\d*\\.\\d* (Inside|Outside)|On) \\d* Stage (Left|Right))");
    var match = regex.Match(drillCoords);

    if (!match.Success)
    {
        throw new Exception("Could not extract drill sets!");
    }

    string x = match.Value;
    string y = drillCoords.Replace(x, "").Trim();

    return (x, y);
}

string text = ExtractText("sample-drill.pdf");
string[] performerChunks = text.Split("\nName:"); //first line begins with Name:, so this splits into performer chunks
performerChunks[0] = performerChunks[0].Replace("Name:", "");

List<Performer> performers = new();
foreach (string chunk in performerChunks)
{
    performers.Add(ExtractPerformer(chunk));
}

Console.WriteLine($"Found {performers.Count} performers");
foreach(Performer performer in performers)
{
    Console.WriteLine($"Performer {performer.Name} ({performer.Label})");
    foreach(DrillSet set in performer.Sets)
    {
        Console.Write($"  Set {set.Identifier}");
        if(set.RehearsalMark != null)
        {
            Console.Write($" (Rehearsal Mark {set.RehearsalMark})");
        }

        Console.WriteLine($": {set.rawCoordsX}, {set.rawCoordsY} with {set.CountsToNextSet} to move");
    }

    Console.WriteLine();
    Console.WriteLine();
}