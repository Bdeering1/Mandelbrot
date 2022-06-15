using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Mandelbrot.Models
{
    public struct BigDecimal
    {
        public BigInteger significand;
        public int exponent;

        public BigDecimal(BigInteger significand, int exponent)
        {
            this.significand = significand;
            this.exponent = exponent;
        }

        /// <summary>
        /// Remove trailing zeroes on significand
        /// </summary>
        public void Normalize()
        {
            throw new NotImplementedException();
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
            a.significand *= -1;
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
            return new BigDecimal(a.significand * b.significand, a.exponent * b.exponent);
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
            return a.exponent == b.exponent && a.significand == b.significand;
        }

        public static bool operator !=(BigDecimal a, BigDecimal b)
        {
            return a.exponent != b.exponent || a.significand != b.significand;
        }


        private static BigDecimal Add(BigDecimal a, BigDecimal b)
        {
            return a.exponent > b.exponent
                ? new BigDecimal(Align(a, b), b.exponent)
                : new BigDecimal(Align(b, a), a.exponent);
        }

        private static BigInteger Align(BigDecimal num, BigDecimal reference)
        {
            return num.significand * BigInteger.Pow(10, num.exponent - reference.exponent);
        }


        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is not null
                && obj is BigDecimal dec
                && dec.exponent == exponent
                && dec.significand == significand;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{significand}e{exponent}";
    }
}