using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Mandelbrot.Models;

namespace Mandelbrot.Core
{
    public class GenerateColors
    {
        //this needs some redoing cuz messy but it works
        public static List<Color> GetGradients(int rangeLightness, int rangeHue)
        {
            var colors = new List<HSL>();

            double ratioL = 1.0 / rangeLightness;
            double ratioH = 1.0 / rangeHue;

            double s = 1.0;

            //generate colors
            for(int i = 0; i < rangeLightness; i++)
            {
                double l = 1.0 - ratioL * i;
                //keep lightness within 0.1-0.7 (dont want black and white for every color)
                l = l * 0.6 + 0.1;
                for(int j = 0; j < rangeHue; j++)
                {
                    double h = ratioH * j;

		            colors.Add(new HSL(h, s, l));
                }
            }
            var sorted = colors.OrderBy(o => o.hue).ThenBy(o => o.lightness).ToList();

            //makes a new list as color objects from the sorted list
            var asColors = new List<Color>();
            for(int i = 0; i < colors.Count(); i++)
            {
                asColors.Add(FromHsl(sorted[i].hue, sorted[i].saturation, sorted[i].lightness));
            }
            return asColors;
        }

        private static Color FromHsl(double hue, double s, double l)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            if (s == 0)
            {
                r = g = b = (int)(l * 255);
            }
            else
            {
                double v1, v2;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (int)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (int)(255 * HueToRGB(v1, v2, hue));
                b = (int)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
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
