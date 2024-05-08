using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text.RegularExpressions;
using System.Text;
using DrillProcessor.Interface;
using DrillProcessor.Model;

namespace DrillProcessor
{
    internal partial class EnvisionDrillExtractor : IDrillExtractor
    {
        public List<RawPerformer> Extract(string file)
        {
            string text = Helpers.ExtractText(file);
            string[] performerChunks = text.Split("\nName:"); //first line begins with Name:, so this splits into performer chunks
            performerChunks[0] = performerChunks[0].Replace("Name:", "");

            List<RawPerformer> performers = new();
            foreach (string chunk in performerChunks)
            {
                performers.Add(ExtractPerformer(chunk));
            }

            return performers;
        }

        public List<RawPerformer> Extract(string file, List<RawPerformer> existingPerformers)
        {
            foreach(RawPerformer performer in Extract(file))
            {
                RawPerformer? match = existingPerformers.FirstOrDefault(p => p.Label == performer.Label);
                if(match != null)
                {
                    match.Sets.AddRange(performer.Sets);
                } else
                {
                    existingPerformers.Add(performer);
                }
            }

            return existingPerformers;
        }

        private static RawPerformer ExtractPerformer(string chunk)
        {
            string[] lines = chunk.Split('\n');
            RawPerformer performer = new(lines[0].Trim(), lines[1].Replace("Label:", "").Trim());

            for (int i = 4; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(" ");

                DrillSet currentSet = new()
                {
                    Identifier = parts[0]
                };

                int whereDoesDrillStartFrom;
                int whereDoesDrillEnd;

                if (int.TryParse(parts[1], out int tmpI))
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
                    int spacesToTrim;
                    if (parts[^1] == "Halt")
                    {
                        spacesToTrim = 2;
                    }
                    else
                    {
                        spacesToTrim = 4; //24, 9 to 5
                    }

                    currentSet.CountsToNextSet = int.Parse(parts[^spacesToTrim]);
                    whereDoesDrillEnd = spacesToTrim;
                }

                string drillCoords = string.Join(" ", parts.Skip(whereDoesDrillStartFrom).SkipLast(whereDoesDrillEnd));
                (currentSet.RawCoordsX, currentSet.RawCoordsY, currentSet.X, currentSet.Y) = ExtractCoords(drillCoords);

                performer.Sets.Add(currentSet);
            }

            return performer;
        }

        static (string x, string y, decimal? x1, decimal? y1) ExtractCoords(string drillCoords)
        {
            if (drillCoords == "You don't stop on this subset")
            {
                return ("", "", null, null);
            }

            Regex regex = SidewaysDrillRegex();
            var match = regex.Match(drillCoords);

            if (!match.Success)
            {
                throw new DrillExtractionException("Could not extract drill sets!");
            }

            string x = match.Value;
            string y = drillCoords.Replace(x, "").Trim();
            decimal xPos = CalculateXPos(match);
            decimal yPos = CalculateYPos(y);

            return (x, y, xPos, yPos);
        }

        static decimal CalculateXPos(Match match)
        {
            string[] coords = match.Groups[2].Value.Split(" ");
            string staging = match.Groups[3].Value;

            int multiplier;
            int marker;
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

        static decimal CalculateYPos(string coords)
        {
            Dictionary<string, decimal> hashes = new()
    {
        {"Front Sideline", 0 },
        {"Front Hash", 32 },
        {"Back Hash", 53.33333M } //blurgh
    };

            Regex regex = DepthDrillRegex();
            var match = regex.Match(coords);

            if (!match.Success)
            {
                throw new DrillExtractionException("How does front to back work?");
            }

            var position = match.Groups[1].Value;
            var hash = match.Groups[2].Value;

            if (!hashes.ContainsKey(hash))
            {
                throw new DrillExtractionException($"Couldn't recognise hash {hash}");
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

        [GeneratedRegex("(((?:(?:\\d*\\.\\d* (?:Inside|Outside))|On) \\d*) ?(Stage (?:Left|Right))?)")]
        private static partial Regex SidewaysDrillRegex();
        [GeneratedRegex("((?:(?:\\d.\\d* (?:In Front|Behind))|On) (.*))")]
        private static partial Regex DepthDrillRegex();
    }
}
