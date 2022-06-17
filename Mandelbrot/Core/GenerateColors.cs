using System.Collections.Generic;
using System.Drawing;

namespace Mandelbrot.Core
{
    public class GenerateColors
    {
        public static List<Color> GetGradients(Color start, Color end, int rangeMax)
        {
            //set max & min RGB values to interpolate between
            int rMax = start.R;
            int rMin = end.R;
            int gMax = start.G;
            int gMin = end.G;
            int bMax = start.B;
            int bMin = end.B;

            var colors = new List<Color>();

            //do the thing (interpolate RGB values then add them to the list)
            for (int i = 0; i < rangeMax; i++)
            {
                var rAverage = rMin + (rMax - rMin) * i / rangeMax;
                var gAverage = gMin + (gMax - gMin) * i / rangeMax;
                var bAverage = bMin + (bMax - bMin) * i / rangeMax;
                colors.Add(Color.FromArgb(rAverage, gAverage, bAverage));
            }
            return colors;
        }
    }
}
