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

        private bool overlap = true;

        private bool showRects = false;
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
            /*
            750x750 image:
            
            Parallel Naive algo
            0 rectangles used
            561775 pixels calculated(99.87 %)
            10.641s elapsed

            Parallel Rects algo
            5168 rectangles used
            58512 pixels calculated (10.4%)
            3.503s elapsed

            2500x2500 image:

            Parallel Naive algo
            0 rectangles used
            6238129 pixels calculated (99.81%)
            85.18s elapsed

            Parallel Rects algo
            15416 rectangles used
            350054 pixels calculated (5.6%)
            34.204s elapsed
            */

            await ComputeSetNaivelyParallel();
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
            var numDivisions = 15;
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

        private async Task ComputeSetRecursiveRectanglesParallel()
        {
            var numDivisions = 15;
            var xDim = width / (float)numDivisions;
            var yDim = height / (float)numDivisions;

            var taskList = new List<Task>();
            int tasks = 0;
            int tasksFinished = 0;

            for (int i = 0; i < numDivisions; i++)
            {
                int newI = i;
                for (int j = 0; j < numDivisions; j++)
                {
                    int newJ = j;
                    tasks++;
                    taskList.Add(Task.Run(() =>
                    {
                        ComputeRectangleRecursively((int)(newI * xDim),
                                                (int)(newJ * yDim),
                                                (int)(newI * xDim + xDim - 1),
                                                (int)(newJ * yDim + yDim - 1));
                        tasksFinished++;
                        Console.WriteLine(tasksFinished + "/" + (i * j));
                    }));
                }
            }
            Console.WriteLine(tasks + " threads started");
            await Task.WhenAll(taskList);
        }

        private void ComputeRectangleRecursively(int leftX, int topY, int rightX, int bottomY)
        {
            int w = rightX - leftX;
            int h = bottomY - topY;
            if (w <= 5 || h <= 5)
            {
                ComputeRect(leftX, topY, rightX, bottomY);
                return;
            }

            var escTime = (int)ComputePixelValue(leftX + w/2, topY + h/2);
            for (int x = leftX; x <= rightX; x += 2)
            {
                uint pt1 = ComputePixelValue(x, topY);
                uint pt2 = ComputePixelValue(x, bottomY);
                if (!(pt1 >= escTime - 2 && pt1 <= escTime + 2)
                    || !(pt2 >= escTime - 2 && pt2 <= escTime + 2))
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }
            for (int y = topY + 1; y < bottomY; y += 2)
            {
                uint pt1 = ComputePixelValue(leftX, y);
                uint pt2 = ComputePixelValue(rightX, y);
                if (!(pt1 >= escTime - 2 && pt1 <= escTime + 2)
                    || !(pt2 >= escTime - 2 && pt2 <= escTime + 2))
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }

            FillRect(leftX, topY, rightX, bottomY, (uint)escTime);
        }

        private void HandleRecursiveCase(int leftX, int topY, int rightX, int bottomY)
        {
            if (overlap)
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                ComputeRectangleRecursively(leftX, topY, leftX + xDim, topY + yDim);
                ComputeRectangleRecursively(leftX + xDim, topY, rightX, topY + yDim);
                ComputeRectangleRecursively(leftX, topY + yDim, leftX + xDim, bottomY);
                ComputeRectangleRecursively(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
            else
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                ComputeRectangleRecursively(leftX, topY, leftX + xDim - 1, topY + yDim - 1);
                ComputeRectangleRecursively(leftX + xDim, topY, rightX, topY + yDim - 1);
                ComputeRectangleRecursively(leftX, topY + yDim, leftX + xDim - 1, bottomY);
                ComputeRectangleRecursively(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
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

        private void FillRect(int leftX, int topY, int rightX, int bottomY, uint escTime)
        {
            var c = colors[(int)escTime - 1];
            uint col = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));

            for(int x = leftX; x <= rightX; x++)
            {
                for(int y = topY; y <= bottomY; y++)
                {
                    if (showRects && (x == leftX || x == rightX || y == topY || y == bottomY))
                    {
                        bytes[y * width + x] = 0x00ff00;
                        escapeTimes[y * width + x] = escTime;
                        continue;
                    }
                    escapeTimes[y * width + x] = escTime;
                    bytes[y * width + x] = col;
                }
            }
        }

        private void ComputeRect(int leftX, int topY, int rightX, int bottomY)
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
        }

        private uint ComputePixelValue(int x, int y)
        {
            var pixelPos = y * width + x;
            if (escapeTimes[pixelPos] != default)
            {
                return escapeTimes[pixelPos];
            }

            pxCalculated++;
            var complexPos = camera.GetComplexPos(x, y);
            if (CheckShapes(complexPos))
            {
                bytes[pixelPos] = 0;
                escapeTimes[pixelPos] = Config.MaxIterations;
                return Config.MaxIterations;
            }

            var escTime = CalcEscapeTime(complexPos);
            var c = colors[(int)escTime - 1];
            bytes[pixelPos] = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));
            escapeTimes[pixelPos] = escTime;

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

        private static uint CalcEscapeTime(BigComplex pt)
        {
            var constant = new BigComplex(pt.r, pt.i);
            var current = BigComplex.Origin;

            var rSquared = (BigDecimal)0;
            var iSquared = (BigDecimal)0;

            uint iter = 0;
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
