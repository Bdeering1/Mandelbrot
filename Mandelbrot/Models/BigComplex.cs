namespace Mandelbrot.Models
{
    public struct BigComplex
    {
        public BigDecimal r;
        public BigDecimal i;

        public BigComplex(BigDecimal real, BigDecimal imaginary)
        {
            r = real;
            i = imaginary;
        }

        public static BigComplex operator +(BigComplex a, BigComplex b) =>
            new BigComplex(a.r + b.r, a.i + b.i);

        public static BigComplex operator -(BigComplex a, BigComplex b) =>
            new BigComplex(a.r - b.r, a.i - b.i);

        public static BigComplex operator *(BigComplex a, BigComplex b) =>
            new BigComplex(a.r * b.r - a.i * b.i,
                        a.r * b.i + a.i * b.r);

        public override string ToString() => $"{r.Truncate()} + {i.Truncate()}i";

        public string ToLongString() => $"{r} + {i}i";
    }
}
