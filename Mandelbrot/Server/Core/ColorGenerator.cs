using System.Drawing;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Core
{
    public class ColorGenerator
    {
        /// <summary>
        /// Creates a list of colors in a gradient from hue 0-360, with lightness steps
        /// </summary>
        /// <param name="maxIter">Number of colors in the gradient</param>
        /// <param name="ratio">Ratio of Hue/Lightness steps</param>
        /// <returns>List of colors</returns>
        public static List<Color> GetGradients(int maxIter, double ratio)
        {
            var colors = new List<Hsl>();

            //Calculate how many iterations we need for each loop
            double rangeLightness = Math.Sqrt((maxIter * (1 - ratio)) / ratio);
            double rangeHue = maxIter / rangeLightness;

            double stepL = 1.0 / (maxIter/(rangeLightness/1.5) * (1 - ratio));
            double stepH = 1.0 / (rangeHue * ratio);

            double s = 1.0;

            //generate colors
            for(int i = 0; i < rangeLightness; i++)
            {
                double l = stepL * (i + 1);
                if (l > 1) l = 1;
                if (l < 0) l = 0;

                for(int j = 0; j < rangeHue; j++)
                {
                    double h = stepH * j;

		            colors.Add(new Hsl(h, s, l));
                }
            }
            //sort the list of colors
            var sorted = colors.OrderBy(o => o.Hue).ThenBy(o => o.Lightness).ToList();

            //makes a new list as color objects from the sorted list
            var asColors = sorted.ConvertAll(new Converter<Hsl, Color>(FromHsl));
            
            return asColors;
        }

        /// <summary>
        /// Creates a list of colors in a gradient from hue 0-360
        /// </summary>
        /// <param name="maxIter"></param>
        /// <returns>List of colors</returns>
        public static List<Color> GetGradients(int maxIter)
        {
            var colors = new List<Hsl>();

            double step = 1.0 / maxIter;
            double s = 1.0;
            double l = 0.5;

            for (int i = 0; i < maxIter; i++)
            {
                double h = step * (i + 1);

                colors.Add(new Hsl(h, s, l));
            }
            
            //sort the list of colors
            var sorted = colors.OrderBy(o => o.Hue).ToList();

            //makes a new list as color objects from the sorted list
            var asColors = sorted.ConvertAll(new Converter<Hsl, Color>(FromHsl));

            return asColors;
        }

        public static string GetHex(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color FromHsl(Hsl hsl)
        {
            int r;
            int g;
            int b;

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
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

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
