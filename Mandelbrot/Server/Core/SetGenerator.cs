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
            var camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 2, width, height);
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

                    if ((complexPos.i < (BigDecimal)0.45 && complexPos.i > (BigDecimal)(-0.14) && complexPos.r < (BigDecimal)0.2 && complexPos.r > (BigDecimal)(-0.564)) ||
                        (complexPos.i < (BigDecimal)0.18 && complexPos.i > (BigDecimal)(-0.064) && complexPos.r < (BigDecimal)(-0.84) && complexPos.r > (BigDecimal)(-1.16)))
                    {
                        set.SetPixel(x, y, new SKColor(colors[^1].R, colors[^1].G, colors[^1].B));
                        continue;
                    };
                    int escTime = CalcEscapeTime(complexPos);
                    set.SetPixel(x, y, new SKColor(colors[escTime - 1].R, colors[escTime - 1].G, colors[escTime - 1].B));
                }
                Console.WriteLine(y);
            }

            for (int y = 1; y < height - reflectHeight; y++)
            {
                if (y == height) break;
                Console.WriteLine($"{reflectHeight + y} (reflected {reflectHeight - y})");
                for (int x = 0; x < width; x++)
                {
                    set.SetPixel(x, reflectHeight + y, set.GetPixel(x, reflectHeight - y));
                }
            }

            return set;
        }

        public static int CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = new BigComplex(new BigDecimal(0), new BigDecimal(0));

            int iter = 0;
            while (current.r * current.r + current.i * current.i <= new BigDecimal(4) && iter < Config.MAX_ITERATIONS)
            {
                current = current * current + constant;
                iter++;
            }
            return iter;
        }
    }
}
