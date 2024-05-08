using DrillProcessor.Model;
using DrillProcessor.Model.Exceptions;

namespace DrillProcessor
{
    internal static class DotExporter
    {
        const string TERMINATOR = "-----";

        public static void Export(List<RawPerformer> performers, string fileTo)
        {
            if (performers.Count == 0) return;

            if (File.Exists(fileTo))
            {
                File.Delete(fileTo);
            }

            Performance performance = TransformPerformers(performers);

            using StreamWriter writer = new(fileTo);

            writer.WriteLine(performance.Title);
            writer.WriteLine(performance.Bpm);

            foreach (PerformanceSet set in performance.Sets)
            {
                writer.WriteLine(set.Identifier);
                writer.WriteLine(set.CountsToNextSet ?? 0);
            }

            writer.WriteLine(TERMINATOR);

            foreach (Performer performer in performance.Performers)
            {
                writer.WriteLine(performer.Name);
                writer.WriteLine(performer.Symbol);

                foreach (Dot dot in performer.Dots)
                {
                    writer.WriteLine($"{dot.X},{dot.Y}");
                }

                writer.WriteLine(TERMINATOR);
            }
        }

        private static Performance TransformPerformers(List<RawPerformer> performers)
        {
            Performance performance = new("Test Drill", 180);

            RawPerformer showTemplate = performers[0];
            foreach (DrillSet drillSet in showTemplate.Sets)
            {
                PerformanceSet set = new()
                {
                    Identifier = drillSet.Identifier,
                    RehearsalMark = drillSet.RehearsalMark,
                    CountsFromLastSet = drillSet.CountsFromLastSet,
                    CountsToNextSet = drillSet.CountsToNextSet
                };

                performance.Sets.Add(set);
            }

            foreach (RawPerformer performer in performers)
            {
                Performer p = new(performer.Name, performer.Label);

                DrillSet? subsetStart = null;
                DrillSet? subsetEnd = null;
                decimal subsetTotal = 0;
                decimal subsetProgress = 0;
                decimal totalMoveX = 0;
                decimal totalMoveY = 0;

                for (int i = 0; i < performer.Sets.Count; i++)
                {
                    Dot? dot = null;
                    if (performer.Sets[i].X == null || performer.Sets[i].Y == null)
                    {
                        //great, we're in a subset
                        //do we have the next set?

                        Console.WriteLine($"Detected subset for {performer.Name} at set {performer.Sets[i].Identifier}");

                        if(subsetStart == null)
                        {
                            subsetStart = performer.Sets[i - 1];
                            subsetTotal = 0;
                        }

                        if (subsetEnd == null)
                        {
                            Console.WriteLine("We don't know where the subset ends :(");
                            //where's the next set?
                            subsetTotal += performer.Sets[i].CountsToNextSet ?? 0;

                            for (int j = i + 1; j < performer.Sets.Count; j++)
                            {
                                subsetTotal += performer.Sets[j].CountsToNextSet ?? 0;

                                if (performer.Sets[j].X != null && performer.Sets[j].Y != null)
                                {
                                    subsetEnd = performer.Sets[j];
                                    Console.WriteLine($"Subset ends at {subsetEnd.Identifier} - {subsetTotal} counts later  - ({subsetEnd.X}, {subsetEnd.Y})");
                                    break;
                                }
                            }

                            if (subsetEnd == null || subsetEnd.X == null || subsetEnd.Y == null)
                            {
                                throw new DrillExportException("Could not find end of subset!");
                            }

                            subsetProgress = performer.Sets[i].CountsFromLastSet ?? 0;
                            totalMoveX = (subsetStart.X!.Value - subsetEnd.X.Value);
                            totalMoveY = (subsetStart.Y!.Value - subsetEnd.Y.Value);
                        }

                        decimal progress = (subsetProgress / subsetTotal);

                        dot = new()
                        {
                            X = subsetStart.X!.Value - (totalMoveX * progress),
                            Y = subsetStart.Y!.Value - (totalMoveY * progress)
                        };
                    }
                    else
                    {
                        subsetStart = null;
                        subsetEnd = null;

                        dot = new()
                        {
                            X = performer.Sets[i].X,
                            Y = performer.Sets[i].Y
                        };
                    }

                    p.Dots.Add(dot);
                }

                performance.Performers.Add(p);
            }

            return performance;
        }
    }
}
