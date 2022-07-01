using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        //not sure if this is the correct way to set class defaults but this is how it is for now
        private BigComplex pos { get; } = new((BigDecimal) 0, (BigDecimal) 0);
        private BigDecimal zoom { get; } = (BigDecimal) 0;

        public Camera(BigComplex pos, BigDecimal zoom)
        {
            this.pos = pos;
            this.zoom = zoom;
        }

        public BigComplex TranslatePixel(int x, int y)
        {
            BigComplex realCoords = PxToCoord(x, y, 400, 400);
            realCoords += pos;
        }

        private BigComplex PxToCoord(int x, int y, int width, int height)
        {
            BigDecimal newX = new BigDecimal(((x * 4) / width) - 2);
            BigDecimal newY = new BigDecimal(((-y * 4) / height) + 2);

            return new(newX, newY);
        }

    }
}
