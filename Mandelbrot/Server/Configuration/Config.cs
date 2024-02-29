using System.Drawing;
using Mandelbrot.Core;

namespace Mandelbrot.Shared.Configuration
{
    public class Config
    {
        public static int ImageWidth { get; set; }
        public static int ImageHeight { get; set; }
        public static int Precision { get; set; }
        public static uint MaxIterations { get; set; }

        public static double Zoom { get; set; }
        public static int Domain { get; set; }
        public static int Range { get; set; }

        public static int ColorTolerance { get; set; }
        public static bool RectOverlap { get; set; }
        public static int InitialDivisions { get; set; }
        public static bool ShowRects { get; set; }

        public static List<Color> Colors { get; set; }

        static Config() => ApplyDefaultConfig();

        public static void ApplyDefaultConfig()
        {
            ImageWidth = 750;
            ImageHeight = 750;
            Precision = 20;
            MaxIterations = 40;
            Zoom = 1;
            Domain = 4;
            Range = 4;
            ColorTolerance = 5;
            RectOverlap = true;
            InitialDivisions = 20;
            ShowRects = false;
            Colors = ColorGenerator.GetBernsteinGradients();
        }

        public static void ApplyMinimalConfig(int precision = 10, uint maxIterations = 100)
        {
            ImageWidth = 100;
            ImageHeight = 100;
            Precision = precision;
            MaxIterations = maxIterations;
            Zoom = 1;
            ColorTolerance = 0;
            RectOverlap = true;
            InitialDivisions = 20;
            ShowRects = false;
            Colors = ColorGenerator.GetBernsteinGradients();
        }
    }
}

