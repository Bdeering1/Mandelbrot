using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        private const int DOMAIN = 4;
        private const int RANGE = 4;

        private BigComplex pos { get; set; } = new((BigDecimal) 0, (BigDecimal) 0);
        private double zoom { get; set; } = 0.0;

        public Camera(BigComplex pos, double zoom)
        {
            this.pos = pos;
            this.zoom = zoom;
        }

        public BigComplex TranslatePixel(int x, int y)
        {
            BigComplex complexPos = PxToCoord(x, y, 500, 500);
            complexPos += pos;
            return complexPos;
        }

        private BigComplex PxToCoord(int x, int y, int width, int height)
        {
            BigDecimal newX = (BigDecimal)(((x * DOMAIN) / (width * zoom)) - (DOMAIN / (2 * zoom)));
            BigDecimal newY = (BigDecimal)(((-y * RANGE) / (height * zoom)) + (RANGE / (2 * zoom)));

            return new(newX, newY);
        }

    }
}
