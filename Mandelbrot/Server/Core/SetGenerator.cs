using System.Drawing;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator
    {
        public static SKBitmap GetBitmap(int width, int height, List<Color> colors)
        {
            var camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1, width, height);
            var set = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);

            int reflectHeight = height;
            var shouldReflect = camera.GetComplexY(0) > (BigDecimal)0 && camera.GetComplexY(height) < (BigDecimal)0;

            for (int y = 0; y < height; y++)
            {
                if (shouldReflect && camera.GetComplexY(y) < (BigDecimal)0)
                {
                    reflectHeight = y - 1;
                    Console.WriteLine($"Reflecting at: {reflectHeight}");
                    y = reflectHeight * 2;
                    shouldReflect = false;
                    continue;
                }
                 
                for (int x = 0; x < width; x++)
                {
                    var complexPos = camera.GetComplexPos(x, y);

                    if(checkShapes(complexPos))
                    {
                        set.SetPixel(x, y, new SKColor(0, 0, 0));
                        continue;
                    }
                    int escTime = CalcEscapeTime(complexPos);
                    set.SetPixel(x, y, new SKColor(colors[escTime - 1].R, colors[escTime - 1].G, colors[escTime - 1].B));
                }
                Console.WriteLine(y);
            }

            for (int y = 1; y < height - reflectHeight; y++)
            {
                if (y == height || reflectHeight - y < 0) break;
                Console.WriteLine($"{reflectHeight + y} (reflected {reflectHeight - y})");
                for (int x = 0; x < width; x++)
                {
                    set.SetPixel(x, reflectHeight + y, set.GetPixel(x, reflectHeight - y));
                }
            }

            return set;
        }

        private static bool checkShapes(BigComplex pos)
        {
            //Cardiod and bulb checking (can be simplified to 1 if statement, currently using two for separating colors
            BigDecimal iSq = pos.i * pos.i;
            BigDecimal a = (pos.r - (BigDecimal)0.25);
            BigDecimal q = a * a + iSq;
            //cardioid check
            if (q * (q + a) < iSq * (BigDecimal)0.25)
            {
                return true;
            }
            //bulb check
            if (((pos.r * pos.r) + ((BigDecimal)2 * pos.r) + (BigDecimal)1 + iSq < (BigDecimal)0.0625))
            {
                return true;
            }
            return false;
        }

        public static int CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = new BigComplex(new BigDecimal(0), new BigDecimal(0));

            var rSquared = (BigDecimal)0;
            var iSquared = (BigDecimal)0;

            int iter = 0;
            while (rSquared + iSquared <= new BigDecimal(4) && iter < Config.MAX_ITERATIONS)
            {
                current = new BigComplex(rSquared - iSquared, current.r * current.i + current.i * current.r) + constant;
                rSquared = current.r * current.r;
                iSquared = current.i * current.i;
                iter++;
            }
            return iter;
        }
    }
}
