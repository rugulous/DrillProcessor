using DrillProcessor;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;

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

    for(int i = 4; i < lines.Length; i++)
    {
        string[] parts = lines[i].Split(" ");
        Console.WriteLine($"Processing set {parts[0]}");

        DrillSet currentSet = new();
        currentSet.Identifier = parts[0];

        if (int.TryParse(parts[1], out tmpI))
        {
            currentSet.CountsFromLastSet = tmpI;
        }
        else
        {
            if (decimal.TryParse(parts[1], out tmpD))
            {
                //start of a drill set
            }
            else if (int.TryParse(parts[2], out tmpI))
            {
                //next part is the counts!
                currentSet.RehearsalMark = parts[1];
                currentSet.CountsFromLastSet = tmpI;
            }
            else
            {

                throw new Exception("Unsure how to proceed");
            }
        }

        performer.Sets.Add(currentSet);
    }

    return performer;
}

string text = ExtractText("sample-drill.pdf");
string[] performerChunks =  text.Split("\nName:"); //first line begins with Name:, so this splits into performer chunks
performerChunks[0] = performerChunks[0].Replace("Name:", "");

List<Performer> performers = new();
foreach(string chunk in performerChunks)
{
    performers.Add(ExtractPerformer(chunk));
}

Console.WriteLine($"Found {performers.Count} performers");