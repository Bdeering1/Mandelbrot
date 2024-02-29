using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {
        public static BigComplex Position { get; set; }

        public Camera()
        {
            Position = BigComplex.Origin;
        }

        public BigComplex GetComplexPos(int x, int y)
        {
            BigComplex complexPos = PxToCoord(x, y);
            complexPos += Position;
            return complexPos;
        }

        private BigComplex PxToCoord(int x, int y)
        {
            var newX = (BigDecimal)(((x * Config.Domain) / (Config.ImageWidth * Config.Zoom)) - (Config.Domain / (2 * Config.Zoom)));
            var newY = (BigDecimal)(((-y * Config.Range) / (Config.ImageHeight * Config.Zoom)) + (Config.Range / (2 * Config.Zoom)));

            return new(newX, newY);
        }
    }
}
