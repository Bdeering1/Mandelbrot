using System.Drawing;
using System.Runtime.InteropServices;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator
    {
        private int width { get; } = Config.IMAGE_WIDTH;
        private int height { get; } = Config.IMAGE_HEIGHT;
        private List<Color> colors { get; set; } = Config.Colors;

        private Camera camera { get; }
        private uint[] bytes { get; set; }
        private uint[] escapeTimes { get; set; }

        private bool showRects = true;
        private int rectsCalculated = 0;
        private int pxCalculated = 0;

        public SetGenerator(Camera camera)
        {
            this.camera = camera;

            bytes = new uint[width * height];
            escapeTimes = new uint[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bytes[y * width + x] = 0xff00ff;
                }
            }
        }
        public async Task<SKBitmap> GetBitmap()
        {
            ComputeSetRecursiveRectangles();
            await Task.Yield();

            var imgInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            //copy byte array to the bitmap for easy handling/compression
            var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned); //makes sure the byte array doesnt get moved in memory, so that the pointer can be used
            bitmap.InstallPixels(imgInfo, gcHandle.AddrOfPinnedObject(), imgInfo.RowBytes, delegate { gcHandle.Free(); }, null);

            //DrawGridLines(bitmap);

            Console.WriteLine($"{rectsCalculated} rectangles used");
            Console.WriteLine($"{pxCalculated} pixels calculated ({pxCalculated / (double)(width * height) * 100:0.##}%)");

            return bitmap;
        }
        private void ComputeSetRecursiveRectangles()
        {
            var numDivisions = 20;
            var xDim = width / (float)numDivisions;
            var yDim = height / (float)numDivisions;

            for (int i = 0; i < numDivisions; i++)
            {
                for (int j = 0; j < numDivisions; j++)
                {
                    ComputeRectangleRecursively((int)(i * xDim),
                                                (int)(j * yDim),
                                                (int)(i * xDim + xDim - 1),
                                                (int)(j * yDim + yDim - 1));
                }
            }
        }

        private void ComputeRectangleRecursively(int leftX, int topY, int rightX, int bottomY)
        {
            bool subdivides = false;

            int w = rightX - leftX;
            int h = bottomY - topY;
            if (w <= 5 || h <= 5)
            {
                for (int x = leftX; x <= rightX; x++)
                {
                    for (int y = topY; y <= bottomY; y++)
                    {
                        ComputePixelValue(x, y);
                        if (showRects && (x == leftX || x == rightX || y == topY || y == bottomY))
                        {
                            bytes[y * width + x] = 0x00ff00;
                        }
                    }
                }
                return;
            }

            int escTime = ComputePixelValue(leftX, topY);
            for (int x = leftX; x <= rightX; x++)
            {
                //calculate escape time for both bottom and top edges (this also sets their color in the byte array)
                int currentEscTimeTop = ComputePixelValue(x, topY);
                int currentEscTimeBottom = ComputePixelValue(x, bottomY);

                if (currentEscTimeTop != escTime || currentEscTimeBottom != escTime)
                {
                    subdivides = true;
                    break;
                }
            }
            //only check other two edges if we didnt already find a different pixel
            if (!subdivides)
            {
                for (int y = topY + 1; y < bottomY; y++)
                {
                    int currentEscTimeLeft = ComputePixelValue(leftX, y);
                    int currentEscTimeRight = ComputePixelValue(rightX, y);

                    if (currentEscTimeLeft != escTime || currentEscTimeRight != escTime)
                    {
                        subdivides = true;
                        break;
                    }
                }
            }

            if (!subdivides)
            {
                FillRect(leftX, topY, rightX, bottomY, escTime);
                return;
            }

            int w2 = (rightX - leftX) / 2;
            int h2 = (bottomY - topY) / 2;
            ComputeRectangleRecursively(leftX, topY, rightX - w2, bottomY - h2);
            ComputeRectangleRecursively(leftX + w2, topY, rightX, bottomY - h2);
            ComputeRectangleRecursively(leftX, topY + h2, rightX - w2, bottomY);
            ComputeRectangleRecursively(leftX + w2, topY + h2, rightX, bottomY);
            rectsCalculated += 4;
        }

        private void ComputeSetNaively()
        {
            int reflectHeight = height;
            var shouldReflect = camera.GetComplexY(0) > (BigDecimal)0 && camera.GetComplexY(height) < (BigDecimal)0;

            for (int y = 0; y < height; y++)
            {
                if (shouldReflect && camera.GetComplexY(y) < (BigDecimal)0)
                {
                    reflectHeight = y - 1;
                    Console.WriteLine($"Reflection point: {reflectHeight}");
                    y = reflectHeight * 2;
                    shouldReflect = false;
                    continue;
                }
                for (int x = 0; x < width; x++)
                {
                    ComputePixelValue(x, y);
                }
                Console.WriteLine(y);
            }

            for (int y = 1; y < height - reflectHeight; y++)
            {
                if (y == height || reflectHeight - y < 0) break;
                Console.WriteLine($"{reflectHeight + y} (from {reflectHeight - y})");
                for (int x = 0; x < width; x++)
                {
                    bytes[(reflectHeight + y) * width + x] = bytes[(reflectHeight - y) * width + x];
                }
            }
        }

        private async Task ComputeSetNaivelyParallel()
        {
            var taskList = new List<Task>();
            for (int y = 0; y < height; y++)
            {
                int capturedY = y;

                taskList.Add(Task.Run(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        ComputePixelValue(x, capturedY);
                    }
                }));
            }
            await Task.WhenAll(taskList);
        }

        private void FillRect(int leftX, int topY, int rightX, int bottomY, int escTime)
        {
            var c = colors[escTime - 1];
            uint col = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));

            for(int x = leftX; x <= rightX; x++)
            {
                for(int y = topY; y <= bottomY; y++)
                {
                    if (showRects && (x == leftX || x == rightX || y == topY || y == bottomY))
                    {
                        bytes[y * width + x] = 0x00ff00;
                        continue;
                    }
                    bytes[y * width + x] = col;
                }
            }
        }

        private int ComputePixelValue(int x, int y)
        {
            var pixelPos = y * width + x;
            if (escapeTimes[pixelPos] != default)
            {
                return (int)escapeTimes[pixelPos];
            }

            pxCalculated++;
            var complexPos = camera.GetComplexPos(x, y);
            if (CheckShapes(complexPos))
            {
                bytes[pixelPos] = 0;
                escapeTimes[pixelPos] = (uint)Config.MaxIterations;
                return Config.MaxIterations;
            }

            int escTime = CalcEscapeTime(complexPos);
            var c = colors[escTime - 1];
            bytes[pixelPos] = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));
            escapeTimes[pixelPos] = (uint)escTime;

            return escTime;
        }

        private void DrawGridLines(SKBitmap set)
        {   
            using var bitmapCanvas = new SKCanvas(set);
            var paint = new SKPaint {
                Color = new SKColor(255, 255, 255)
            };
            var paintCenter = new SKPaint
            {
                Color = new SKColor(255, 0, 0)
            };
            const int gridSpacing = 200;
            var centerX = width / 2;
            var centerY = height / 2;
            var xDif = gridSpacing;
            var yDif = gridSpacing;

            bitmapCanvas.DrawLine(new SKPoint(centerX, 0), new SKPoint(centerX, height), paintCenter);
            bitmapCanvas.DrawLine(new SKPoint(0, centerY), new SKPoint(width, centerY), paintCenter);


            while (centerX - xDif > 0)
            {
                bitmapCanvas.DrawLine(new SKPoint(centerX - xDif, 0), new SKPoint(centerX - xDif, height), paint);
                bitmapCanvas.DrawLine(new SKPoint(centerX + xDif, 0), new SKPoint(centerX + xDif, height), paint);
                xDif += gridSpacing;
            }
            while (centerY - yDif > 0)
            {
                bitmapCanvas.DrawLine(new SKPoint(0, centerY - yDif), new SKPoint(width, centerY - yDif), paint);
                bitmapCanvas.DrawLine(new SKPoint(0, centerY + yDif), new SKPoint(width, centerY + yDif), paint);
                yDif += gridSpacing;
            }
        }

        private static int CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = BigComplex.Origin;

            var rSquared = (BigDecimal)0;
            var iSquared = (BigDecimal)0;

            int iter = 0;
            while (rSquared + iSquared <= new BigDecimal(4) && iter < Config.MaxIterations)
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
            var iSquared = pos.i * pos.i;
            var a = pos.r - (BigDecimal)0.25;
            var q = a * a + iSquared;
            if (q * (q + a) < iSquared * (BigDecimal)0.25
                || (pos.r * pos.r) + ((BigDecimal)2 * pos.r) + (BigDecimal)1 + iSquared < (BigDecimal)0.0625)
            {
                return true;
            }
            return false;
        }
    }
}
