using System.Drawing;
using System.Runtime.InteropServices;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using SkiaSharp;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator
    {
        private int width { get; } = Config.ImageWidth;
        private int height { get; } = Config.ImageHeight;

        private EscapeTime escapeTime { get; }
        private uint[] bytes { get; set; }
        private uint[] escapeTimes { get; set; }

        private int tasksStarted;
        private int tasksCompleted;

        private int rectsCalculated = 0;
        private int pxCalculated = 0;

        public SetGenerator(EscapeTime escapeTime)
        {
            this.escapeTime = escapeTime;

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

        public void Reset()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    escapeTimes[y * width + x] = default;
                }
            }
            tasksStarted = 0;
            tasksCompleted = 0;
            rectsCalculated = 0;
            pxCalculated = 0;
    }

        private void ComputeSetRecursiveRectangles()
        {
            var numDivisions = Config.InitialDivisions;
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
            var numDivisions = Config.InitialDivisions;
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
            Console.WriteLine($"Max threads: {Config.MaxThreads} Threads completed: {tasksCompleted}");
        }

        private void ComputeRectangleRecursivelyThreadOption(int leftX, int topY, int rightX, int bottomY)
        {
            if (tasksStarted - tasksCompleted < Config.MaxThreads)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((object? obj) =>
                {
                    tasksStarted++;
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
                if (Math.Abs(ComputePixelValue(x, topY) - escTime) > Config.ColorTolerance
                    || Math.Abs(ComputePixelValue(x, bottomY) - escTime) > Config.ColorTolerance)
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }
            for (int y = topY + 1; y < bottomY; y += 2)
            {
                if (Math.Abs(ComputePixelValue(leftX, y) - escTime) > Config.ColorTolerance
                    || Math.Abs(ComputePixelValue(rightX, y) - escTime) > Config.ColorTolerance)
                {
                    HandleRecursiveCase(leftX, topY, rightX, bottomY);
                    return;
                }
            }

            FillRect(leftX, topY, rightX, bottomY, escTime);
        }

        private void HandleRecursiveCase(int leftX, int topY, int rightX, int bottomY)
        {
            if (Config.RectOverlap)
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
            var shouldReflect = Camera.GetComplexY(0) > (BigDecimal)0 && Camera.GetComplexY(height) < (BigDecimal)0;

            for (int y = 0; y < height; y++)
            {
                if (shouldReflect && Camera.GetComplexY(y) < (BigDecimal)0)
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
            var c = Config.Colors[(int)escTime - 1];
            uint col = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));

            for(int x = leftX; x <= rightX; x++)
            {
                for(int y = topY; y <= bottomY; y++)
                {
                    if (Config.ShowRects && (x == leftX || x == rightX || y == topY || y == bottomY))
                    {
                        escapeTimes[y * width + x] = escTime;
                        bytes[y * width + x] = 0x00ff00;
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
                    if (Config.ShowRects && (x == leftX || x == rightX || y == topY || y == bottomY))
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
            var complexPos = Camera.GetComplexPos(x, y);
            if (escapeTime.CheckShapes(complexPos))
            {
                bytes[pixelPos] = 0;
                escapeTimes[pixelPos] = Config.MaxIterations;
                return Config.MaxIterations;
            }

            var escTime = escapeTime.CalcEscapeTime(complexPos);
            var c = Config.Colors[(int)escTime - 1];
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
    }
}
