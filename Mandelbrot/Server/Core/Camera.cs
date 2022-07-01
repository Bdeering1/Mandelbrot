using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        private BigComplex pos { get; } = new((BigDecimal) 0, (BigDecimal) 0);
        private double zoom { get; } = 0.0;

        public Camera(BigComplex pos, double zoom)
        {
            this.pos = pos;
            this.zoom = zoom;
        }

        public BigComplex TranslatePixel(int x, int y)
        {
            BigComplex realCoords = PxToCoord(x, y, 400, 400);
            realCoords += pos;
            return realCoords;
        }

        private BigComplex PxToCoord(int x, int y, int width, int height)
        {
            BigDecimal newX = (BigDecimal)(((x * 4) / (width * zoom)) - (4 / (2 * zoom)));
            BigDecimal newY = (BigDecimal)(((-y * 4) / (height * zoom)) + (4 / (2 * zoom)));

            return new(newX, newY);
        }

    }
}
