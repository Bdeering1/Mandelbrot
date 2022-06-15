using System;
using Value_Objects;

namespace Mandelbrot
{
    public class EscapeTime
    {
        public static int CalcEscapeTime(Complex pt)
        {
            var constant = new Complex(pt.r, pt.i);
            var current = new Complex(0, 0);

            int iter = 0;
            int maxIterations = 200;

            while(Math.Sqrt((Math.Pow(current.r, 2) + Math.Pow(current.i, 2))) <= 2 && iter <= maxIterations)
            {
                current = current * current + constant;
                iter++;
            }
            return iter;
        }
    }
}
