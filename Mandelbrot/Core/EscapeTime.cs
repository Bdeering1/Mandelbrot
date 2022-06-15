using System;
using Mandelbrot.Value_Objects;

namespace Mandelbrot.Core
{
    public class EscapeTime
    {
        public static int CalcEscapeTime(Complex pt)
        {
            var constant = new Complex(pt.r, pt.i);
            var current = new Complex(0, 0);

            int iter = 0;
            int maxIter = 200;

            while (Math.Sqrt(Math.Pow((double)current.r, 2) + Math.Pow((double)current.i, 2)) <= 2 && iter <= maxIter)
            {
                current = current * current + constant;
                iter++;
            }
            return iter;
        }
    }
}
