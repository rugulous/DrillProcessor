namespace DrillProcessor
{
    internal static class DotExporter
    {
        const string TERMINATOR = "-----";

        public static void Export(List<RawPerformer> performers, string fileTo)
        {
            if(performers.Count == 0) return;

            if(File.Exists(fileTo))
            {
                File.Delete(fileTo);
            }

            Performance performance = TransformPerformers(performers);

            using (StreamWriter writer = new(fileTo))
            {
                writer.WriteLine(performance.Title);
                writer.WriteLine(performance.Bpm);

                foreach(PerformanceSet set in performance.Sets)
                {
                    writer.WriteLine(set.Identifier);
                    writer.WriteLine(set.CountsToNextSet ?? 0);
                }

                writer.WriteLine(TERMINATOR);

                foreach(Performer performer in performance.Performers)
                {
                    writer.WriteLine(performer.Name);
                    writer.WriteLine(performer.Symbol);

                    foreach(Dot dot in performer.Dots)
                    {
                        writer.WriteLine($"{dot.X},{dot.Y}");
                    }

                    writer.WriteLine(TERMINATOR);
                }

                //writer.WriteLine(performer.Name);
                //writer.WriteLine(performer.Label);

                //foreach(DrillSet set in performer.Sets)
                //{
                //    writer.WriteLine(set.Identifier);
                //    writer.WriteLine(set.CountsToNextSet ?? 0);
                //    if(set.X == null || set.Y == null)
                //    {
                //        writer.WriteLine("-");
                //    } else
                //    {
                //        writer.WriteLine($"{set.X},{set.Y}");
                //    }
                //}
            }
        }

        private static Performance TransformPerformers(List<RawPerformer> performers)
        {
            Performance performance = new("Test Drill", 180);

            RawPerformer showTemplate = performers[0];
            foreach(DrillSet drillSet in showTemplate.Sets)
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

            foreach(RawPerformer performer in performers)
            {
                Performer p = new(performer.Name, performer.Label);
                p.Dots = performer.Sets.ConvertAll(s => new Dot()
                {
                    X = s.X,
                    Y = s.Y
                });

                performance.Performers.Add(p);
            }

            return performance;
        }
    }
}
