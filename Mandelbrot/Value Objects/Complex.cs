namespace Value_Objects
{
    public struct Complex
    {
        public Complex(decimal real, decimal imaginary)
        {
            r = real;
            i = imaginary;
        }

        public decimal r;
        public decimal i;

        public static Complex operator +(Complex a, Complex b)
        {
            a.r += b.r;
            a.i += b.i;
            return a;
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.r * b.r - a.i * b.i, a.r * b.i + a.i * b.r);
        }
   
}
