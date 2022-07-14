using System.Drawing;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class GenerateSet
    {
        public static List<Color> GetColorList(List<Color> colors, int width, int height)
        {
            Camera camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1.0);

            var set = new List<Color>();
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BigComplex realPos = camera.TranslatePixel(x + 1, y + 1);

                    if (realPos.i < (BigDecimal)0.4 && realPos.i > (BigDecimal)(-0.4) && realPos.r < (BigDecimal)0.2 && realPos.r > (BigDecimal)(-0.2))
                    {
                        set.Add(colors[colors.Count - 1]);
                        continue;
                    };
                    var escTime = EscapeTime.CalcEscapeTime(realPos);
                    set.Add(colors[escTime - 1]);
                }
                Console.WriteLine(y);
            }

            Console.WriteLine("Done!");

            return set;
        }

        public static SKBitmap GetBitmap(List<Color> colors, int width, int height)
        {
            Camera camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1.0);

            var set = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BigComplex realPos = camera.TranslatePixel(x + 1, y + 1);

                    if (realPos.i < (BigDecimal)0.4 && realPos.i > (BigDecimal)(-0.4) && realPos.r < (BigDecimal)0.2 && realPos.r > (BigDecimal)(-0.2))
                    {
                        set.SetPixel(x, y, new SKColor(colors[^1].R, colors[^1].G, colors[^1].B));
                        continue;
                    };
                    int escTime = EscapeTime.CalcEscapeTime(realPos);
                    set.SetPixel(x, y, new SKColor(colors[escTime - 1].R, colors[escTime - 1].G, colors[escTime - 1].B));
                }
                Console.WriteLine(y);
            }

            Console.WriteLine("Done!");

            return set;
        }
    }
}
