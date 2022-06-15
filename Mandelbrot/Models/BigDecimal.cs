using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Mandelbrot.Models
{
    /// <summary>
    /// Arbitrary precision decimal
    /// Based on https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d
    /// </summary>
    public struct BigDecimal
    {
        public static int Precision { get; set; } = 50;

        public BigInteger Significand { get; set; }
        public int Exponent { get; set; }

        public BigDecimal(BigInteger significand, int exponent)
        {
            Significand = significand;
            Exponent = exponent;
        }

        /// <summary>
        /// Remove trailing zeroes on significand
        /// </summary>
        public void Normalize()
        {
            if (Exponent == 0) return;
            if (Significand == 0)
            {
                Exponent = 0;
                return;
            }
            BigInteger remainder = 0;
            while (remainder == 0)
            {
                var shortened = BigInteger.DivRem(Significand, 10, out remainder);
                if (remainder == 0)
                {
                    Significand = shortened;
                    Exponent++;
                }
            }
        }

        /// <summary>
        /// Truncate to the given precision by removing least significant digits
        /// </summary>
        public void Truncate()
        {
            throw new NotImplementedException();
        }


        public static BigDecimal operator -(BigDecimal a)
        {
            a.Significand *= -1;
            return a;
        }

        public static BigDecimal operator +(BigDecimal a, BigDecimal b)
        {
            return Add(a, b);
        }

        public static BigDecimal operator -(BigDecimal a, BigDecimal b)
        {
            return Add(a, -b);
        }

        public static BigDecimal operator *(BigDecimal a, BigDecimal b)
        {
            return new BigDecimal(a.Significand * b.Significand, a.Exponent * b.Exponent);
        }

        public static BigDecimal operator /(BigDecimal a, BigDecimal b)
        {
            throw new NotImplementedException();
        }

        public static BigDecimal Sqrt(BigDecimal a)
        {
            throw new NotImplementedException();
        }


        public static bool operator ==(BigDecimal a, BigDecimal b)
        {
            return a.Exponent == b.Exponent && a.Significand == b.Significand;
        }

        public static bool operator !=(BigDecimal a, BigDecimal b)
        {
            return a.Exponent != b.Exponent || a.Significand != b.Significand;
        }


        private static BigDecimal Add(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent
                ? new BigDecimal(Align(a, b), b.Exponent)
                : new BigDecimal(Align(b, a), a.Exponent);
        }

        private static BigInteger Align(BigDecimal num, BigDecimal reference)
        {
            return num.Significand * BigInteger.Pow(10, num.Exponent - reference.Exponent);
        }


        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is not null && obj is BigDecimal dec && dec == this;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{Significand}e{Exponent}";
    }
}