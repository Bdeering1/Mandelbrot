namespace Value_Objects
{
    public struct Complex
    {
        public double r;
        public double i;

        public Complex(double real, double imaginary)
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
