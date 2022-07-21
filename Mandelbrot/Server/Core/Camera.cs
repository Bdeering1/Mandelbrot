using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        private int domain { get; }
        private int range { get; } = 4;

        private int imageSizeX { get; }
        private int imageSizeY { get; }

        private BigComplex position { get; set; } = new((BigDecimal) 0, (BigDecimal) 0);
        private double zoom { get; set; } = 0.0;

        public Camera(BigComplex position, double zoom, int imageSizeX, int imageSizeY)
        {
            this.position = position;
            this.zoom = zoom;
            this.imageSizeX = imageSizeX;
            this.imageSizeY = imageSizeY;
            domain = (int)(range * (imageSizeX / (float)imageSizeY));
        }

        public BigComplex GetComplexPos(int x, int y)
        {
            BigComplex complexPos = PxToCoord(x, y);
            complexPos += position;
            return complexPos;
        }

        public BigDecimal GetComplexY(int y)
        {
            var newY = (BigDecimal)(((-y * range) / (imageSizeY * zoom)) + (range / (2 * zoom)));
            return newY + position.i;
        }

        private BigComplex PxToCoord(int x, int y)
        {
            var newX = (BigDecimal)(((x * domain) / (imageSizeX * zoom)) - (domain / (2 * zoom)));
            var newY = (BigDecimal)(((-y * range) / (imageSizeY * zoom)) + (range / (2 * zoom)));

            return new(newX, newY);
        }
    }
}
