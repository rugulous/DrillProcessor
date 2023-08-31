namespace DrillProcessor
{
    internal static class DotExporter
    {
        public static void Export(Performer performer, string fileTo)
        {
            if(File.Exists(fileTo))
            {
                File.Delete(fileTo);
            }

            using (StreamWriter writer = new(fileTo))
            {
                writer.WriteLine(performer.Name);
                writer.WriteLine(performer.Label);

                foreach(DrillSet set in performer.Sets)
                {
                    writer.WriteLine(set.Identifier);
                    writer.WriteLine(set.CountsToNextSet ?? 0);
                    if(set.X == null || set.Y == null)
                    {
                        writer.WriteLine("-");
                    } else
                    {
                        writer.WriteLine($"{set.X},{set.Y}");
                    }
                }
            }
        }
    }
}
