using System.Drawing;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator
    {
        public static SKBitmap GetBitmap(List<Color> colors, int width, int height)
        {
            var camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1);

            var set = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BigComplex complexPos = camera.TranslatePixel(x + 1, y + 1);

                    if ((complexPos.i < (BigDecimal)0.45 && complexPos.i > (BigDecimal)(-0.14) && complexPos.r < (BigDecimal)0.2 && complexPos.r > (BigDecimal)(-0.564)) ||
                        (complexPos.i < (BigDecimal)0.18 && complexPos.i > (BigDecimal)(-0.064) && complexPos.r < (BigDecimal)(-0.84) && complexPos.r > (BigDecimal)(-1.16)))
                    {
                        set.SetPixel(x, y, new SKColor(colors[^1].R, colors[^1].G, colors[^1].B));
                        set.SetPixel(x, height - y - 1, new SKColor(colors[^1].R, colors[^1].G, colors[^1].B));
                        continue;
                    };
                    int escTime = CalcEscapeTime(complexPos);
                    set.SetPixel(x, y, new SKColor(colors[escTime - 1].R, colors[escTime - 1].G, colors[escTime - 1].B));
                    set.SetPixel(x, height - y - 1, new SKColor(colors[escTime - 1].R, colors[escTime - 1].G, colors[escTime - 1].B));
                }
                Console.WriteLine(y);
            }

            return set;
        }

        public static int CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = new BigComplex(new BigDecimal(0), new BigDecimal(0));

            int iter = 0;
            int maxIter = 200;

            while (current.r * current.r + current.i * current.i <= new BigDecimal(4) && iter < maxIter)
            {
                current = current * current + constant;
                iter++;
            }
            return iter;
        }
    }
}
