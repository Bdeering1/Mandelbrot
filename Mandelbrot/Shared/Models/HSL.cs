namespace Mandelbrot.Models
{
    public struct HSL
    {
        public HSL(double h, double s, double l)
        {
            hue = h;
            saturation = s;
            lightness = l;
        }
        public double hue;
        public double saturation;
        public double lightness;
    }
}
