using System.Drawing;
using Mandelbrot.Core;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Shared.Configuration
{
    public static class Config
    {
        public const int IMAGE_WIDTH = 1400;
        public const int IMAGE_HEIGHT = 800;

        public static BigComplex DefaultCameraPosition { get; } = BigComplex.Origin;
        public static double DefaultCameraZoom { get; } = 1.0;

        public static List<Color> Colors { get; } = ColorGenerator.GetBernsteinGradients();

        public static int MaxIterations { get; } = 100;
    }
}

