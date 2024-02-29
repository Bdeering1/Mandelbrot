using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Mandelbrot.Shared.Models
{
    /// <summary>
    /// Arbitrary precision decimal<br/>
    /// Based on https://gist.github.com/JcBernack/0b4eef59ca97ee931a2f45542b9ff06d
    /// </summary>
    public struct BigDecimal
    {
        public static int Precision { get; set; } = 25;

        public BigInteger Significand { get; private set; }
        public int Exponent { get; private set; }


        public BigDecimal(BigInteger significand, int exponent = 0, bool truncate = false)
        {
            Significand = significand;
            Exponent = exponent;

            Normalize();

            if (truncate) Truncate();
        }

        public BigDecimal(decimal num)
        {
            Exponent = 0;
            while (num % 1 != 0)
            {
                num *= 10;
                Exponent--;
            }
            Significand = (BigInteger)num;

            Normalize();
        }

        /// <summary>
        /// Remove trailing zeroes on significand
        /// </summary>
        public void Normalize()
        {
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
        public BigDecimal Truncate()
        {
            Normalize();

            var numDigs = NumberOfDigits(Significand);
            var digsToRemove = Math.Max(0, numDigs - Precision);
            Significand /= BigInteger.Pow(10, digsToRemove);
            Exponent += digsToRemove;

            Normalize();
            return this;
        }


        public static explicit operator BigDecimal(int num) => new(num);

        public static explicit operator BigDecimal(long num) => new(num);

        public static explicit operator BigDecimal(float num) => new((decimal)num);

        public static explicit operator BigDecimal(double num) => new((decimal)num);

        public static explicit operator BigDecimal(decimal num) => new(num);

        public static explicit operator float(BigDecimal value) =>
            Convert.ToSingle((double)value);

        public static explicit operator double(BigDecimal value) =>
            (double)value.Significand * Math.Pow(10, value.Exponent);

        public static explicit operator decimal(BigDecimal value) =>
            (decimal)value.Significand * (decimal)Math.Pow(10, value.Exponent);


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
            return new BigDecimal(a.Significand * b.Significand, a.Exponent + b.Exponent, true);
        }


        public static bool operator ==(BigDecimal a, BigDecimal b)
        {
            return a.Exponent == b.Exponent && a.Significand == b.Significand;
        }

        public static bool operator !=(BigDecimal a, BigDecimal b)
        {
            return a.Exponent != b.Exponent || a.Significand != b.Significand;
        }

        public static bool operator <(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent ? Align(a, b) < b.Significand : a.Significand < Align(b, a);
        }

        public static bool operator >(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent ? Align(a, b) > b.Significand : a.Significand > Align(b, a);
        }

        public static bool operator <=(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent ? Align(a, b) <= b.Significand : a.Significand <= Align(b, a);
        }

        public static bool operator >=(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent ? Align(a, b) >= b.Significand : a.Significand >= Align(b, a);
        }


        private static BigDecimal Add(BigDecimal a, BigDecimal b)
        {
            return a.Exponent > b.Exponent
                ? new BigDecimal(Align(a, b) + b.Significand, b.Exponent)
                : new BigDecimal(Align(b, a) + a.Significand, a.Exponent);
        }

        private static BigInteger Align(BigDecimal num, BigDecimal reference)
        {
            return num.Significand * BigInteger.Pow(10, num.Exponent - reference.Exponent);
        }

        public static int NumberOfDigits(BigInteger num)
        {
            return num == 0
                ? 1
                : num < 0
                ? (int)Math.Floor(BigInteger.Log10(-num) + 1)
                : (int)Math.Floor(BigInteger.Log10(num) + 1);
        }


        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is not null && obj is BigDecimal dec && dec == this;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{Significand}e{Exponent} ({NumberOfDigits(Significand)}) ";
    }
}