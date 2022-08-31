using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Shared.Configuration
{
    public class Config
    {
        public const int IMAGE_WIDTH = 750;
        public const int IMAGE_HEIGHT = 750;

        public const int COLOR_TOLERANCE = 5;
        public const int MAX_THREADS = 7;

        public static int Precision { get; } = 20;
        public static uint MaxIterations { get; } = 40;

        public static BigComplex Position { get; } = BigComplex.Origin;
        public static double Zoom { get; } = 1.0;

        public static bool RectOverlap { get; } = true;
        public static int InitialDivisions { get; } = 20;
        public static bool ShowRects { get; } = false;

        public static List<Color> Colors { get; } = ColorGenerator.GetBernsteinGradients();
    }
}

