namespace Mandelbrot.Shared.Models
{
    public class BigComplex
    {
        public BigDecimal r { get; private set; }
        public BigDecimal i { get; private set; }

        public BigComplex(BigDecimal real, BigDecimal imaginary)
        {
            r = real;
            i = imaginary;
        }
        public BigComplex(double real, double imaginary)
        {
            r = (BigDecimal)real;
            i = (BigDecimal)imaginary;
        }
        public static BigComplex Origin = new(0, 0);

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
