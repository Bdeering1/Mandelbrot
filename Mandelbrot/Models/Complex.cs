namespace Mandelbrot.Models
{
    public struct Complex
    {
        public decimal r;
        public decimal i;

        public Complex(decimal real, decimal imaginary)
        {
            r = real;
            i = imaginary;
        }

        public static Complex operator +(Complex a, Complex b) =>
            new Complex(a.r + b.r, a.i + b.i);

        public static Complex operator -(Complex a, Complex b) =>
            new Complex(a.r - b.r, a.i - b.i);

        public static Complex operator *(Complex a, Complex b) =>
            new Complex(a.r * b.r - a.i * b.i,
                        a.r * b.i + a.i * b.r);

        public override string ToString() => $"{r} + {i}i";
    }
}
