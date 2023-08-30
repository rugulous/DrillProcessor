namespace DrillProcessor
{
    internal static class Helpers
    {
        public static int Scale(decimal num, int scale)
        {
            return (int)(num * scale);
        }

        public static int Scale(double num, int scale)
        {
            return (int) (num * scale);
        }

        public static int Scale(int num, int scale)
        {
            return num * scale;
        }
    }
}
