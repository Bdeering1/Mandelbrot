using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        public BigComplex position { get; set; }
        public double zoom { get; set; }

        private int domain { get; }
        private int range { get; } = 4;

        private int imageSizeX { get; }
        private int imageSizeY { get; }


        public Camera()
        {
            imageSizeX = Config.ImageWidth;
            imageSizeY = Config.ImageHeight;
            position = Config.Position;
            zoom = Config.Zoom;

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
