using DrillProcessor;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

const decimal PACES_TO_YARDS = 1.875M;

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

        if (i == lines.Length - 1)
        {
            //last line has no out / step size
            whereDoesDrillEnd = 0;
        }
        else
        {
            int spacesToTrim = -1;
            if (parts[parts.Length - 1] == "Halt")
            {
                spacesToTrim = 2;
            }
            else
            {
                spacesToTrim = 4; //24, 9 to 5
            }

            currentSet.CountsToNextSet = int.Parse(parts[parts.Length - spacesToTrim]);
            whereDoesDrillEnd = spacesToTrim;
        }

        string drillCoords = string.Join(" ", parts.Skip(whereDoesDrillStartFrom).SkipLast(whereDoesDrillEnd));
        (currentSet.rawCoordsX, currentSet.rawCoordsY, currentSet.X, currentSet.Y) = ExtractCoords(drillCoords);

        performer.Sets.Add(currentSet);
    }

    return performer;
}

(string x, string y, decimal? x1, decimal? y1) ExtractCoords(string drillCoords)
{
    if (drillCoords == "You don't stop on this subset")
    {
        return (null, null, null, null);
    }

    Regex regex = new("(((?:(?:\\d*\\.\\d* (?:Inside|Outside))|On) \\d*) ?(Stage (?:Left|Right))?)");
    var match = regex.Match(drillCoords);

    if (!match.Success)
    {
        throw new Exception("Could not extract drill sets!");
    }

    string x = match.Value;
    string y = drillCoords.Replace(x, "").Trim();
    decimal xPos = CalculateXPos(match);
    decimal yPos = CalculateYPos(y);

    return (x, y, xPos, yPos);
}

decimal CalculateXPos(Match match)
{
    string[] coords = match.Groups[2].Value.Split(" ");
    string staging = match.Groups[3].Value;

    int multiplier = 0;
    int marker = 0;
    decimal steps = 0;
    if (staging == "Stage Left")
    {
        multiplier = -1;
    }
    else if (staging == "Stage Right")
    {
        multiplier = 1;
    }
    else
    {
        //on 50
        multiplier = 0;
    }

    if (coords[0] == "On")
    {
        marker = int.Parse(coords[1]); //On 50
    }
    else
    {
        marker = int.Parse(coords[2]); //xx outside 50
        steps = decimal.Parse(coords[0]) * (coords[1] == "Outside" ? 1 : -1);
    }

    marker = Math.Abs(50 - marker) / 5; //calc distance from 50: 40 = 2, 30 = 4, etc 
    steps += marker * 8; //coordinate system sees 8 paces inbetween each marker

    return steps * multiplier;
}

decimal CalculateYPos(string coords)
{
    Dictionary<string, decimal> hashes = new()
    {
        {"Front Sideline", 0 },
        {"Front Hash", 32 },
        {"Back Hash", 53.33333M } //blurgh
    };

    Regex regex = new("((?:(?:\\d.\\d* (?:In Front|Behind))|On) (.*))");
    var match = regex.Match(coords);

    if (!match.Success)
    {
        throw new Exception("How does front to back work?");
    }

    var position = match.Groups[1].Value;
    var hash = match.Groups[2].Value;

    if (!hashes.ContainsKey(hash))
    {
        throw new Exception($"Couldn't recognise hash {hash}");
    }

    decimal steps = hashes[hash];
    if (!position.StartsWith("On"))
    {
        decimal posPart = decimal.Parse(coords.Split(" ")[0]);

        if (position.Contains("In Front"))
        {
            steps -= posPart;
        }
        else
        {
            steps += posPart;
        }
    }

    return steps;
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
Pen pen = new(Color.FromArgb(130, 160, 160, 160), 5);
foreach (Performer performer in performers)
{
    Bitmap bmp = new(1600, 906);
    Graphics graphics = Graphics.FromImage(bmp);
    graphics.FillRectangle(Brushes.DarkGreen, 0, 0, 1600, 53);
    graphics.FillRectangle(Brushes.Green, 0, 53, 1600, 853);
    graphics.FillRectangle(Brushes.White, 0, 53, 1600, 1);

    for (int i = 0; i <= 20; i++)
    {
        graphics.FillRectangle(Brushes.White, i * 80, 53, (i == 10 ? 5 : 1), 853);
        graphics.FillRectangle(Brushes.White, (i * 80) - 20, 373, 40, 1);
        graphics.FillRectangle(Brushes.White, (i * 80) - 20, 586, 40, 1);

    }

    for (int j = 0; j < 530; j += 80)
    {
        graphics.FillEllipse(Brushes.White, 797, 48 + j, 10, 10);
    }

    int? lastX = null;
    int? lastY = null;

    Console.WriteLine($"Performer {performer.Name} ({performer.Label})");
    foreach (DrillSet set in performer.Sets)
    {
        Console.Write($"  Set {set.Identifier}");
        if (set.RehearsalMark != null)
        {
            Console.Write($" (Rehearsal Mark {set.RehearsalMark})");
        }

        Console.WriteLine($": {set.rawCoordsX}, {set.rawCoordsY} with {set.CountsToNextSet} to move || {set.X}, {set.Y}");

        if (set.X != null && set.Y != null)
        {
            int x = 800 + ((int)set.X * 10);
            int y = 53 + ((int)set.Y * 10);

            if (lastX != null && lastY != null)
            {
                graphics.DrawLine(pen, lastX.Value, lastY.Value, x, y);
            }

            graphics.FillEllipse(Brushes.Black, x - 3, y - 3, 10, 10);

            lastX = x;
            lastY = y;
        }
    }

    bmp.Save($"C:\\temp\\drill\\{performer.Label}.bmp");
    Console.WriteLine();
    Console.WriteLine();
}
