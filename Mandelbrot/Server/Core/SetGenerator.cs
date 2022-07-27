using System.Drawing;
using System.Runtime.InteropServices;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Color> Colors { get; }

        private Camera camera { get; set; }
        private uint[] set { get; set; }

        public SetGenerator(int width, int height, List<Color> colors)
        {
            Width = width;
            Height = height;
            Colors = colors;

            set = new uint[width * height];
            camera = new Camera(new BigComplex((BigDecimal)0, (BigDecimal)0), 1, Width, Height);
        }

        public SKBitmap GetBitmap()
        {
            var imgInfo = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            var bitmap = new SKBitmap(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            int reflectHeight = Height;
            var shouldReflect = camera.GetComplexY(0) > (BigDecimal)0 && camera.GetComplexY(Height) < (BigDecimal)0;

            for (int y = 0; y < Height; y++)
            {
                if (shouldReflect && camera.GetComplexY(y) < (BigDecimal)0)
                {
                    reflectHeight = y - 1;
                    Console.WriteLine($"Reflection point: {reflectHeight}");
                    y = reflectHeight * 2;
                    shouldReflect = false;
                    continue;
                }
                for (int x = 0; x < Width; x++)
                {
                    ComputePixelValue(x, y);
                }
                Console.WriteLine(y);
            }

            for (int y = 1; y < Height - reflectHeight; y++)
            {
                if (y == Height || reflectHeight - y < 0) break;
                Console.WriteLine($"{reflectHeight + y} (from {reflectHeight - y})");
                for (int x = 0; x < Width; x++)
                {
                    set[(reflectHeight + y) * Width + x] = set[(reflectHeight - y) * Width + x];
                }
            }

            //copy byte array to the bitmap for easy handling/compression
            var gcHandle = GCHandle.Alloc(set, GCHandleType.Pinned); //makes sure the byte array doesnt get moved in memory, so that the pointer can be used
            bitmap.InstallPixels(imgInfo, gcHandle.AddrOfPinnedObject(), imgInfo.RowBytes, delegate { gcHandle.Free(); }, null);

            DrawCenterLines(bitmap);

            return bitmap;
        }

        private void ComputePixelValue(int x, int y)
        {
            var complexPos = camera.GetComplexPos(x, y);

            if (CheckShapes(complexPos))
            {
                set[y * Width + x] = 0;
                return;
            }
            int escTime = CalcEscapeTime(complexPos);
            var c = Colors[escTime - 1];
            set[y * Width + x] = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));
        }

        private void DrawCenterLines(SKBitmap set)
        {
            using var bitmapCanvas = new SKCanvas(set);
            var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255)
            };
            bitmapCanvas.DrawLine(new SKPoint(0, Height / 2), new SKPoint(Width, Height / 2), paint);
            bitmapCanvas.DrawLine(new SKPoint(Width / 2, 0), new SKPoint(Width / 2, Height), paint);
        }

        private static int CalcEscapeTime(BigComplex pt)
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

        private static bool CheckShapes(BigComplex pos)
        {
            BigDecimal iSquared = pos.i * pos.i;
            BigDecimal a = pos.r - (BigDecimal)0.25;
            BigDecimal q = a * a + iSquared;
            if (q * (q + a) < iSquared * (BigDecimal)0.25
                || (pos.r * pos.r) + ((BigDecimal)2 * pos.r) + (BigDecimal)1 + iSquared < (BigDecimal)0.0625)
            {
                return true;
            }
            return false;
        }
    }
}
