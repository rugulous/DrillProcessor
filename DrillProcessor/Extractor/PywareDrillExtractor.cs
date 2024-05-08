using DrillProcessor.Interface;
using DrillProcessor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrillProcessor
{
    public partial class PywareDrillExtractor : IDrillExtractor
    {
        //TODO: this bit is almost identical to EnvisionDrillExtractor
        //maybe it's just the ExtractPerformer that needs to be variable
        public List<RawPerformer> Extract(string file)
        {
            string text = Helpers.ExtractText(file, twoToAPage: HasTwoPerPage(file)); //hmm... maybe a callback here instead?
            Console.WriteLine(text);

            string[] performerChunks = text.Split("\nPerformer:"); //first line begins with Name:, so this splits into performer chunks
            performerChunks[0] = performerChunks[0].Replace("Performer:", "");

            List<RawPerformer> performers = new();
            foreach (string chunk in performerChunks)
            {
                performers.Add(ExtractPerformer(chunk));
            }

            return performers;
        }

        private static RawPerformer ExtractPerformer(string chunk)
        {
            string[] lines = chunk.Split('\n');
            string[] nameParts = lines[0].Split(":");
            RawPerformer performer = new(nameParts[0].Replace("Symbol", "").Trim(), nameParts[2].Replace("ID", "").Trim());

            for (int i = 2; i < lines.Length - 1; i++) //last line is Page x of y
            {
                string[] parts = lines[i].Split(" ");
                if (parts.Length < 10)
                {
                    continue;
                }

                int drillStartsFrom = 4; //sometimes an extra space is added for centering!
                int counts;

                if (!int.TryParse(parts[3], out counts))
                {
                    if (int.TryParse(parts[4], out counts))
                    {
                        drillStartsFrom = 5;
                    }
                    else
                    {
                        drillStartsFrom = 6;
                        counts = int.Parse(parts[5]);
                    }
                }

                DrillSet currentSet = new()
                {
                    Identifier = parts[0],
                    CountsToNextSet = counts
                };


                string drillCoords = string.Join(" ", parts[drillStartsFrom..parts.Length]);
                (currentSet.RawCoordsX, currentSet.RawCoordsY, currentSet.X, currentSet.Y) = ExtractCoords(drillCoords);

                performer.Sets.Add(currentSet);
            }

            for(int i = 0; i < performer.Sets.Count - 1; i++)
            {
                performer.Sets[i].CountsToNextSet = performer.Sets[i + 1].CountsToNextSet;
            }

            performer.Sets[performer.Sets.Count - 1].CountsToNextSet = 0;

            return performer;
        }

        //unsure if Pyware can export one per page, but this check doesn't hurt anyone!
        private bool HasTwoPerPage(string file)
        {
            string firstPage = Helpers.ExtractText(file, 1);
            return Regex.Matches(firstPage, "Performer:").Count > 1;
        }


        static (string x, string y, decimal? x1, decimal? y1) ExtractCoords(string drillCoords)
        {
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
            string staging = match.Groups[1].Value; //[1] side [2] steps [3] in/out [4] yardline

            int multiplier;
            int marker = int.Parse(match.Groups[4].Value);

            decimal steps;
            if (decimal.TryParse(match.Groups[2].Value, out steps))
            {
                if (match.Groups[3].Value == "inside")
                {
                    steps = -steps;
                }
            }
            else
            {
                steps = 0;
            }

            if (staging == "Right")
            {
                multiplier = -1;
            }
            else if (staging == "Left")
            {
                multiplier = 1;
            }
            else
            {
                //on 50
                multiplier = 0;
            }

            marker = Math.Abs(50 - marker) / 5; //calc distance from 50: 40 = 2, 30 = 4, etc 
            steps += marker * 8; //coordinate system sees 8 paces inbetween each marker

            return steps * multiplier;
        }

        static decimal CalculateYPos(string coords)
        {
            Dictionary<string, decimal> hashes = new()
    {
        {"Home side line", 0 },
        {"Home Hash", 32 },
        {"Visitor Hash", 53.33333M } //blurgh
    };

            Regex regex = DepthDrillRegex();
            var match = regex.Match(coords);

            if (!match.Success)
            {
                throw new DrillExtractionException("How does front to back work?");
            }

            var position = match.Groups[2].Value.ToLower(); // [1] steps [2] front/behind [3] home/visitor hash/sideline
            var hash = match.Groups[3].Value;

            if (!hashes.ContainsKey(hash))
            {
                throw new DrillExtractionException($"Couldn't recognise hash {hash}");
            }

            decimal steps = hashes[hash];
            if (position != "on")
            {
                decimal posPart = decimal.Parse(match.Groups[1].Value);

                if (position == "in front")
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

        [GeneratedRegex("(?:(Left|Right): )?(?:([\\d.]*) steps )?(inside|outside|On) ([\\d]*) yd ln")]
        private static partial Regex SidewaysDrillRegex();
        [GeneratedRegex("(?:([\\d.]*) steps )?(in front|behind|On) (?:of )?((?:Home|Visitor) (?:Hash|side line))")]
        private static partial Regex DepthDrillRegex();
    }
}
