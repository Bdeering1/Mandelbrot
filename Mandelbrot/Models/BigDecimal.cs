﻿using System;
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

        public BigDecimal(BigInteger significand, int exponent = 0)
        {
            Significand = significand;
            Exponent = exponent;

            Normalize();
        }

        public BigDecimal(double num)
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
        public void Truncate()
        {
            Normalize();
            while(NumberOfDigits(Significand) > Precision)
            {
                Significand /= 10;
                Exponent++;
            }
            Normalize();
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
                ? new BigDecimal(Align(a, b) + b.Significand, b.Exponent)
                : new BigDecimal(Align(b, a) + a.Significand, a.Exponent);
        }

        private static BigInteger Align(BigDecimal num, BigDecimal reference)
        {
            return num.Significand * BigInteger.Pow(10, num.Exponent - reference.Exponent);
        }

        private static int NumberOfDigits(BigInteger num)
        {
            return num.Sign == -1 ? num.ToString().Length - 1 : num.ToString().Length;
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