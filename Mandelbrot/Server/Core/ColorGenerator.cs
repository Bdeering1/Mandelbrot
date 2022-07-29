using System.Drawing;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Core
{
    public static class ColorGenerator
    {
        /// <summary>
        /// Creates a list of colors in a gradient from hue 0-360
        /// </summary>
        /// <returns>List of colors</returns>
        public static List<Color> GetGradients()
        {
            var colors = new List<Hsl>();

            double s = 1.0;
            double l = 0.5;

            for (int i = 0; i < Config.MaxIterations; i++)
            {
                double h = (double) i / Config.MaxIterations;
                colors.Add(new Hsl(h, s, l));
            }
            
            //sort the list of colors
            var sorted = colors.OrderBy(o => o.Hue).ToList();
            sorted[sorted.Count - 1] = new Hsl(1.0, 1.0, 0.0);

            //makes a new list as color objects from the sorted list
            var asColors = sorted.ConvertAll(new Converter<Hsl, Color>(FromHsl));
            return asColors;
        }

        /// <summary>
        /// Creates a list of colors in a gradient from hue 0-360
        /// </summary>
        /// <returns>List of colors</returns>
        public static List<Color> GetBernsteinGradients()
        {
            var colors = new List<Color>();

            for (int i = 0; i < Config.MaxIterations; i++)
            {
                double t = (double)i / Config.MaxIterations;
                colors.Add(Color.FromArgb(
                    1,
                    (int)(9 * (1 - t) * Math.Pow(t, 3) * 255),
                    (int)(15 * Math.Pow(1 - t, 2) * Math.Pow(t, 2) * 255),
                    (int)(8.5 * Math.Pow(1 - t, 3) * t * 255)
                    ));
            }
            colors[(int)Config.MaxIterations - 1] = Color.FromArgb(0,0,0,0);

            return colors;
        }

        public static string GetHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color FromHsl(Hsl hsl)
        {
            int r, g, b;

            if (hsl.Saturation == 0)
            {
                r = g = b = (int)(hsl.Lightness * 255);
            }
            else
            {
                double v1, v2;

                v2 = (hsl.Lightness < 0.5) ? (hsl.Lightness * (1 + hsl.Saturation)) : ((hsl.Lightness + hsl.Saturation) - (hsl.Lightness * hsl.Saturation));
                v1 = 2 * hsl.Lightness - v2;

                r = (int)(255 * HueToRGB(v1, v2, hsl.Hue + (1.0f / 3)));
                g = (int)(255 * HueToRGB(v1, v2, hsl.Hue));
                b = (int)(255 * HueToRGB(v1, v2, hsl.Hue - (1.0f / 3)));
            }

            return Color.FromArgb(0, r, g, b);
        }

        private static double HueToRGB(double v1, double v2, double vH)
        {
            if (vH < 0) vH += 1;

            if (vH > 1) vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
    }
}
