using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        private const int DOMAIN = 6;
        private const int RANGE = 4;

        private int domainPx { get; }
        private int rangePx { get; }

        private BigComplex position { get; set; } = new((BigDecimal) 0, (BigDecimal) 0);
        private double zoom { get; set; } = 0.0;

        public Camera(BigComplex position, double zoom, int domainPx, int rangePx)
        {
            this.position = position;
            this.zoom = zoom;
            this.domainPx = domainPx;
            this.rangePx = rangePx;
        }

        public BigComplex GetComplexPos(int x, int y)
        {
            BigComplex complexPos = PxToCoord(x, y);
            complexPos += position;
            return complexPos;
        }

        public BigDecimal GetComplexY(int y)
        {
            var newY = (BigDecimal)(((-y * RANGE) / (rangePx * zoom)) + (RANGE / (2 * zoom)));
            return newY + position.i;
        }

        private BigComplex PxToCoord(int x, int y)
        {
            var newX = (BigDecimal)(((x * DOMAIN) / (domainPx * zoom)) - (DOMAIN / (2 * zoom)));
            var newY = (BigDecimal)(((-y * RANGE) / (rangePx * zoom)) + (RANGE / (2 * zoom)));

            return new(newX, newY);
        }
    }
}
