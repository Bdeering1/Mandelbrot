using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Shared.Configuration
{
    public static class Config
    {
        public const int IMAGE_WIDTH = 750;
        public const int IMAGE_HEIGHT = 750;

        public static BigComplex DefaultCameraPosition { get; } = BigComplex.Origin;
        public static double DefaultCameraZoom { get; } = 1.0;

        public static int Precision { get; } = 25;
        public static uint MaxIterations { get; } = 100;

        public static List<Color> Colors { get; } = ColorGenerator.GetBernsteinGradients();
    }
}

