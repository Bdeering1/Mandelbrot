using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;

namespace Mandelbrot.Server.Core
{
    public class Camera
    {

        public static BigComplex GetComplexPos(int x, int y)
        {
            BigComplex complexPos = PxToCoord(x, y);
            complexPos += Config.Position;
            return complexPos;
        }

        public static BigDecimal GetComplexY(int y)
        {
            var newY = (BigDecimal)(((-y * Config.range) / (Config.ImageHeight * Config.Zoom)) + (Config.range / (2 * Config.Zoom)));
            return newY + Config.Position.i;
        }

        private static BigComplex PxToCoord(int x, int y)
        {
            var newX = (BigDecimal)(((x * Config.domain) / (Config.ImageWidth * Config.Zoom)) - (Config.domain / (2 * Config.Zoom)));
            var newY = (BigDecimal)(((-y * Config.range) / (Config.ImageHeight * Config.Zoom)) + (Config.range / (2 * Config.Zoom)));

            return new(newX, newY);
        }
    }
}
