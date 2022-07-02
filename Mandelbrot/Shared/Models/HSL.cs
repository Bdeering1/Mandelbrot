namespace Mandelbrot.Shared.Models
{
    public class Hsl
    {
        public double Hue { get; set; }
        public double Saturation { get; set; }
        public double Lightness { get; set; }

        public Hsl(double h, double s, double l)
        {
            Hue = h;
            Saturation = s;
            Lightness = l;
        }
    }
}
