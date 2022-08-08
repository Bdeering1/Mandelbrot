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

        private List<Task> tasks = new();
        private int tasksStarted;
        private int tasksCompleted;

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

            tasksStarted = int.MaxValue;
            tasksCompleted = 0;
        }

        public async Task<SKBitmap> GetBitmap()
        {
            await ComputeSetRecursiveRectanglesParallel();
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

        private async Task ComputeSetRecursiveRectanglesParallel()
        {
            var numDivisions = 20;
            var xDim = width / (float)numDivisions;
            var yDim = height / (float)numDivisions;
            tasksStarted = 0;

            for (int x = 0; x < numDivisions; x++)
            {
                int newX = x;
                for (int y = 0; y < numDivisions; y++)
                {
                    int newY = y;
                    ComputeRectangleRecursivelyThreadOption((int)(newX * xDim),
                                                            (int)(newY * yDim),
                                                            (int)(newX * xDim + xDim - 1),
                                                            (int)(newY * yDim + yDim - 1));
                }
            }
            await Task.Yield();
            await Task.WhenAll(tasks);
            Console.WriteLine($"Max threads: {Config.MAX_THREADS} Threads completed: {tasksCompleted}");
        }

        private void ComputeRectangleRecursivelyThreadOption(int leftX, int topY, int rightX, int bottomY)
        {
            if (tasksStarted - tasksCompleted < Config.MAX_THREADS)
            {
                tasksStarted++;
                tasks.Add(Task.Run(() =>
                {
                    //Console.WriteLine($"{tasksStarted - tasksCompleted}/{maxTasks} ({tasksCompleted} total)");
                    ComputeRectangleRecursively(leftX, topY, rightX, bottomY);
                    tasksCompleted++;
                }));
            }
            else
            {
                ComputeRectangleRecursively(leftX, topY, rightX, bottomY);
            }
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

            var escTime = ComputePixelValue(leftX + w/2, topY + h/2);
            for (int x = leftX; x <= rightX; x += 2)
            {
                if (Math.Abs(ComputePixelValue(x, topY) - escTime) > Config.COLOR_TOLERANCE
                    || Math.Abs(ComputePixelValue(x, bottomY) - escTime) > Config.COLOR_TOLERANCE)
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }
            for (int y = topY + 1; y < bottomY; y += 2)
            {
                if (Math.Abs(ComputePixelValue(leftX, y) - escTime) > Config.COLOR_TOLERANCE
                    || Math.Abs(ComputePixelValue(rightX, y) - escTime) > Config.COLOR_TOLERANCE)
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }

            FillRect(leftX, topY, rightX, bottomY, escTime);
        }

        private void HandleRecursiveCase(int leftX, int topY, int rightX, int bottomY)
        {
            if (Config.Overlap)
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                ComputeRectangleRecursivelyThreadOption(leftX, topY, leftX + xDim, topY + yDim);
                ComputeRectangleRecursivelyThreadOption(leftX + xDim, topY, rightX, topY + yDim);
                ComputeRectangleRecursivelyThreadOption(leftX, topY + yDim, leftX + xDim, bottomY);
                ComputeRectangleRecursivelyThreadOption(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
            else
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                ComputeRectangleRecursivelyThreadOption(leftX, topY, leftX + xDim - 1, topY + yDim - 1);
                ComputeRectangleRecursivelyThreadOption(leftX + xDim, topY, rightX, topY + yDim - 1);
                ComputeRectangleRecursivelyThreadOption(leftX, topY + yDim, leftX + xDim - 1, bottomY);
                ComputeRectangleRecursivelyThreadOption(leftX + xDim, topY + yDim, rightX, bottomY);
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
