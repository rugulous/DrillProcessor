using System.Drawing;

namespace DrillProcessor
{
    internal class DotPlotter
    {
        const int FIELD_WIDTH = 160;
        const decimal FIELD_HEIGHT = 90.6M;
        const decimal PIT_BOX = 5.3M;

        const int STEPS_PER_MARKER = 8;

        private Bitmap? Field { get; }
        private (int?, int?) LastCoords { get; set; }
        private int Scale { get; }
        private Pen? GhostTrail { get; }

        public DotPlotter(int scale)
        {
            Scale = scale;

            int width = Helpers.Scale(FIELD_WIDTH, Scale);
            int height = Helpers.Scale(FIELD_HEIGHT, Scale);

            if (OperatingSystem.IsWindows())
            {
                Field = new(width, height);
                using Graphics graphics = Graphics.FromImage(Field);

                DrawField(width, height, graphics);
                GhostTrail = new(Color.FromArgb(130, 160, 160, 160), scale / 2);
            }
        }

        ~DotPlotter()
        {
            if (OperatingSystem.IsWindows())
            {
                GhostTrail?.Dispose();
                Field?.Dispose();
            }
        }

        public Bitmap? PlotDots(Performer performer)
        {
            if (!OperatingSystem.IsWindows() || Field == null)
            {
                return null;
            }

            LastCoords = (null, null);

            Bitmap bitmap = new(Field);
            using Graphics graphics = Graphics.FromImage(bitmap);

            foreach (DrillSet drill in performer.Sets)
            {
                DrawSet(drill, graphics);
            }

            //Rotate 180 so it looks like drill books
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

            return bitmap;
        }

        private void DrawSet(DrillSet set, Graphics graphics)
        {
            if (!OperatingSystem.IsWindows() || set.X == null || set.Y == null || GhostTrail == null)
            {
                return;
            }

            int x = Helpers.Scale(80 + set.X.Value, Scale);
            int y = Helpers.Scale(PIT_BOX + set.Y.Value, Scale);

            if (LastCoords.Item1 != null && LastCoords.Item2 != null)
            {
                graphics.DrawLine(GhostTrail, LastCoords.Item1.Value, LastCoords.Item2.Value, x, y);
            }

            graphics.FillEllipse(Brushes.Black, x - (Scale / 2), y - (Scale / 2), Scale, Scale);

            LastCoords = (x, y);
        }

        private void DrawField(int width, int height, Graphics graphics)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            int frontSideline = Helpers.Scale(PIT_BOX, Scale);

            graphics.FillRectangle(Brushes.DarkGreen, 0, 0, width, frontSideline);
            graphics.FillRectangle(Brushes.Green, 0, frontSideline, width, height - frontSideline);
            graphics.FillRectangle(Brushes.White, 0, frontSideline, width, Scale / 10);

            for (int i = 0; i <= 20; i++)
            {
                int hashPos = Helpers.Scale((i * STEPS_PER_MARKER) - 2, Scale);
                graphics.FillRectangle(Brushes.White, Helpers.Scale(i * 8, Scale), frontSideline, Helpers.Scale(i == 10 ? 5 : 1, Scale / 10), height - frontSideline);
                graphics.FillRectangle(Brushes.White, hashPos, Helpers.Scale(37.3, Scale), Helpers.Scale(4, Scale), Helpers.Scale(1, Scale / 10));
                graphics.FillRectangle(Brushes.White, hashPos, Helpers.Scale(58.6, Scale), Helpers.Scale(4, Scale), Helpers.Scale(1, Scale / 10));

                for (int j = 1; j < 8; j++)
                {
                    graphics.FillRectangle(Brushes.White, Helpers.Scale((i * STEPS_PER_MARKER) + j, Scale), frontSideline, Scale / 10, Scale);
                }
            }

            for (int i = 0; i < 53 * Scale; i += 8 * Scale)
            {
                graphics.FillEllipse(Brushes.White, Helpers.Scale(79.7, Scale), Helpers.Scale(4.8, Scale) + i, Scale, Scale);
            }
        }
    }
}
