using System.Drawing;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Core
{
    public class ColorGenerator
    {
        /// <summary>
        /// Creates a list of colors in a gradient from hue 0-360
        /// </summary>
        /// <param name="maxIter">Number of colors in the gradient</param>
        /// <param name="ratio">Ratio of Hue/Lightness steps</param>
        /// <returns>List of colors</returns>
        public static List<Color> GetGradients(int maxIter, double ratio)
        {
            var colors = new List<HSL>();

            //Calculate how many iterations we need for each loop
            double rangeLightness = Math.Sqrt((maxIter * (1 - ratio)) / ratio);
            double rangeHue = maxIter / rangeLightness;

            double ratioL = 1.0 / (maxIter/(rangeLightness/1.5) * (1 - ratio));
            double ratioH = 1.0 / (rangeHue * ratio);

            double s = 1.0;

            //generate colors
            for(int i = 0; i < rangeLightness; i++)
            {
                double l = ratioL * (i + 1);
                if (l > 1) l = 1;
                if (l < 0) l = 0;

                for(int j = 0; j < rangeHue; j++)
                {
                    double h = ratioH * j;

		            colors.Add(new HSL(h, s, l));
                }
            }
            //sort the list of colors
            var sorted = colors.OrderBy(o => o.hue).ThenBy(o => o.lightness).ToList();

            //makes a new list as color objects from the sorted list
            var asColors = sorted.ConvertAll(new Converter<HSL, Color>(FromHsl));
            
            return asColors;
        }

        public static string GetHexString(Color c) =>
        $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color FromHsl(HSL hsl)
        {
            int r;
            int g;
            int b;

            if (hsl.saturation == 0)
            {
                r = g = b = (int)(hsl.lightness * 255);
            }
            else
            {
                double v1, v2;

                v2 = (hsl.lightness < 0.5) ? (hsl.lightness * (1 + hsl.saturation)) : ((hsl.lightness + hsl.saturation) - (hsl.lightness * hsl.saturation));
                v1 = 2 * hsl.lightness - v2;

                r = (int)(255 * HueToRGB(v1, v2, hsl.hue + (1.0f / 3)));
                g = (int)(255 * HueToRGB(v1, v2, hsl.hue));
                b = (int)(255 * HueToRGB(v1, v2, hsl.hue - (1.0f / 3)));
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
