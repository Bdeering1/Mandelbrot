﻿using System;
using Mandelbrot.Models;

namespace Mandelbrot.Core
{
    public class EscapeTime
    {
        public static int CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = new BigComplex(new BigDecimal(0), new BigDecimal(0));

            int iter = 0;
            int maxIter = 5000;

            while (current.r * current.r + current.i * current.i <= new BigDecimal(4) && iter < maxIter)
            {
                current = current * current + constant;
                Console.WriteLine($"{current}");
                iter++;
            }
            return iter;
        }
    }
}