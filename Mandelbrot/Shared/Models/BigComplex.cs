namespace Mandelbrot.Shared.Models
{
    public struct BigComplex
    {
        public BigDecimal R { get; private set; }
        public BigDecimal I { get; private set; }


        public BigComplex(BigDecimal real, BigDecimal imaginary)
        {
            R = real;
            I = imaginary;
        }
        public BigComplex(double real, double imaginary)
        {
            R = (BigDecimal)real;
            I = (BigDecimal)imaginary;
        }

        public static readonly BigComplex Origin = new(0, 0);

        public static BigComplex operator +(BigComplex a, BigComplex b) =>
            new BigComplex(a.R + b.R, a.I + b.I);

        public static BigComplex operator -(BigComplex a, BigComplex b) =>
            new BigComplex(a.R - b.R, a.I - b.I);

        public static BigComplex operator *(BigComplex a, BigComplex b) =>
            new BigComplex(a.R * b.R - a.I * b.I,
                        a.R * b.I + a.I * b.R);


        public override string ToString() => $"{R.Truncate()} + {I.Truncate()}i";

        public string ToLongString() => $"{R} + {I}i";
    }
}
