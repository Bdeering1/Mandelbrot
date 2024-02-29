using System.Drawing;
using System.Runtime.InteropServices;
using Mandelbrot.Shared.Configuration;
using Mandelbrot.Shared.Models;
using SkiaSharp;
using ILGPU;
using ILGPU.Runtime;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mandelbrot.Server.Core
{
    public class SetGenerator : IDisposable
    {
        private int width { get; } = Config.ImageWidth;
        private int height { get; } = Config.ImageHeight;

        private EscapeTime escapeTime { get; }
        private Camera Camera { get; }
        private uint[] bytes { get; set; }
        private uint[] escapeTimes { get; set; }

        private int tasksStarted;
        private int tasksCompleted;

        private int rectsCalculated = 0;
        private int pxCalculated = 0;

        // Variables for running on GPU
        private readonly bool useCPU = false;
        private static Context? context;
        private static Accelerator? accelerator;
        struct GPUParams
        {
            public int domain;
            public int range;
            public int width;
            public int height;
            public double zoom;
            public double posR;
            public double posI;
            public uint maxIterations;
            public ArrayView<uint> output;

            public GPUParams(int domain, int range, int width, int height, double zoom, double posR, double posI, uint maxIterations, ArrayView<uint> output)
            {
                this.domain = domain;
                this.range = range;
                this.width = width;
                this.height = height;
                this.zoom = zoom;
                this.posR = posR;
                this.posI = posI;
                this.maxIterations = maxIterations;
                this.output = output;
            }
        }
        private static Action<Index1D, GPUParams>? generatorKernel;

        public SetGenerator(EscapeTime escapeTime, Camera camera)
        {
            this.escapeTime = escapeTime;
            this.Camera = camera;

            bytes = new uint[width * height];
            escapeTimes = new uint[width * height];

            context = Context.CreateDefault();
            var preferredDevice = context.GetPreferredDevice(false);
            if (preferredDevice.AcceleratorType == AcceleratorType.CPU) {
                useCPU = true;
            } else {
                accelerator = preferredDevice.CreateAccelerator(context);
                generatorKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, GPUParams>(GeneratorKernel);
            }

            Reset();
        }

        public async Task<SKBitmap> GetBitmap()
        {
            if (useCPU) {
                await ComputeParallel();
            } else {
                escapeTimes = CalcGPU(width, height);
            }
            await Task.Yield();

            for (int i = 0; i < width * height; i++)
            {
                var escTime = escapeTimes[i];
                var c = Config.Colors[(int)escTime - 1];
                bytes[i] = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | (c.R << 0));
            }

            var imgInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));

            //copy byte array to the bitmap for easy handling/compression
            var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned); //makes sure the byte array doesnt get moved in memory, so that the pointer can be used
            bitmap.InstallPixels(imgInfo, gcHandle.AddrOfPinnedObject(), imgInfo.RowBytes, delegate { gcHandle.Free(); }, null);

            //DrawGrid(bitmap);
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
                    bytes[y * width + x] = 0xff00ff;
                }
            }
            tasksStarted = 0;
            tasksCompleted = 0;
            rectsCalculated = 0;
            pxCalculated = 0;
        }

        private void Compute()
        {
            var numDivisions = Config.InitialDivisions;
            var xDim = width / (float)numDivisions;
            var yDim = height / (float)numDivisions;

            for (int i = 0; i < numDivisions; i++)
            {
                for (int j = 0; j < numDivisions; j++)
                {
                    ComputeRectangle((int)(i * xDim),
                                                (int)(j * yDim),
                                                (int)(i * xDim + xDim - 1),
                                                (int)(j * yDim + yDim - 1));
                }
            }
        }

        private void ComputeRectangle(int leftX, int topY, int rightX, int bottomY)
        {
            int w = rightX - leftX;
            int h = bottomY - topY;
            if (w <= 5 || h <= 5)
            {
                ComputeRect(leftX, topY, rightX, bottomY);
                return;
            }

            var escTime = ComputePixelValue(leftX + w / 2, topY + h / 2);
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
                ComputeRectangle(leftX, topY, leftX + xDim, topY + yDim);
                ComputeRectangle(leftX + xDim, topY, rightX, topY + yDim);
                ComputeRectangle(leftX, topY + yDim, leftX + xDim, bottomY);
                ComputeRectangle(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
            else
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                ComputeRectangle(leftX, topY, leftX + xDim - 1, topY + yDim - 1);
                ComputeRectangle(leftX + xDim, topY, rightX, topY + yDim - 1);
                ComputeRectangle(leftX, topY + yDim, leftX + xDim - 1, bottomY);
                ComputeRectangle(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
        }


        private async Task ComputeParallel()
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
                    QueueRectangle((int)(newX * xDim),
                                                            (int)(newY * yDim),
                                                            (int)(newX * xDim + xDim - 1),
                                                            (int)(newY * yDim + yDim - 1));
                }
            }
            await Task.Yield();
            while (tasksStarted > tasksCompleted)
            {
                Thread.Sleep(5);
            }
            Console.WriteLine($"Max threads: {Config.MaxThreads} Threads completed: {tasksCompleted}");
        }

        private void QueueRectangle(int leftX, int topY, int rightX, int bottomY)
        {
            tasksStarted++;
            ThreadPool.QueueUserWorkItem(new WaitCallback((object? obj) =>
            {
                ComputeRectangleParallel(leftX, topY, rightX, bottomY);
                tasksCompleted++;
            }));
        }

        private void ComputeRectangleParallel(int leftX, int topY, int rightX, int bottomY)
        {
            int w = rightX - leftX;
            int h = bottomY - topY;
            if (w <= 5 || h <= 5)
            {
                ComputeRect(leftX, topY, rightX, bottomY);
                return;
            }

            var escTime = ComputePixelValue(leftX + w / 2, topY + h / 2);
            for (int x = leftX; x <= rightX; x += 2)
            {
                if (Math.Abs(ComputePixelValue(x, topY) - escTime) > Config.ColorTolerance
                    || Math.Abs(ComputePixelValue(x, bottomY) - escTime) > Config.ColorTolerance)
                {
                    HandleRecursiveCaseParallel(leftX, topY, rightX, bottomY);
                    return;
                }
            }
            for (int y = topY + 1; y < bottomY; y += 2)
            {
                if (Math.Abs(ComputePixelValue(leftX, y) - escTime) > Config.ColorTolerance
                    || Math.Abs(ComputePixelValue(rightX, y) - escTime) > Config.ColorTolerance)
                {
                    HandleRecursiveCaseParallel(leftX, topY, rightX, bottomY);
                    return;
                }
            }

            FillRect(leftX, topY, rightX, bottomY, escTime);
        }

        private void HandleRecursiveCaseParallel(int leftX, int topY, int rightX, int bottomY)
        {
            if (Config.RectOverlap)
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                QueueRectangle(leftX, topY, leftX + xDim, topY + yDim);
                QueueRectangle(leftX + xDim, topY, rightX, topY + yDim);
                QueueRectangle(leftX, topY + yDim, leftX + xDim, bottomY);
                QueueRectangle(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
            else
            {
                var xDim = (rightX - leftX + 1) / 2;
                var yDim = (bottomY - topY + 1) / 2;
                QueueRectangle(leftX, topY, leftX + xDim - 1, topY + yDim - 1);
                QueueRectangle(leftX + xDim, topY, rightX, topY + yDim - 1);
                QueueRectangle(leftX, topY + yDim, leftX + xDim - 1, bottomY);
                QueueRectangle(leftX + xDim, topY + yDim, rightX, bottomY);
                rectsCalculated += 4;
            }
        }


        private async Task ComputeNaivelyParallel()
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

            for (int x = leftX; x <= rightX; x++)
            {
                for (int y = topY; y <= bottomY; y++)
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

        private static uint[] CalcGPU(int width, int height)
        {
            if (accelerator == null || generatorKernel == null) { return Array.Empty<uint>(); }

            Console.WriteLine("started generating on GPU");
            MemoryBuffer1D<uint, Stride1D.Dense> GPU_out = accelerator.Allocate1D<uint>(width * height);
            var gp = new GPUParams(
                Config.Domain,
                Config.Range,
                width,
                height,
                Config.Zoom,
                (double)Camera.Position.R,
                (double)Camera.Position.I,
                Config.MaxIterations,
                GPU_out.View
            );
            generatorKernel((int)GPU_out.Length, gp);

            Console.WriteLine("waiting for accelerator to synchronize...");
            accelerator.Synchronize();

            Console.WriteLine("finished generating on GPU");
            uint[] bytesOut = GPU_out.GetAsArray1D();
            return bytesOut;
        }

        private static void GeneratorKernel(Index1D index, GPUParams gp)
        {
            int x = index % gp.width;
            int y = index / gp.height;
            double constR = x * gp.domain / (gp.width * gp.zoom) - (gp.domain / (2 * gp.zoom)) + gp.posR;
            double constI = -y * gp.range / (gp.height * gp.zoom) + (gp.range / (2 * gp.zoom)) + gp.posI;

            double curR = 0;
            double curI = 0;

            double rSquared = 0;
            double iSquared = 0;

            uint iter = 0;
            while (rSquared + iSquared <= 4 && iter < gp.maxIterations)
            {
                double tempR = rSquared - iSquared + constR;
                curI = 2 * curR * curI + constI;
                curR = tempR;

                rSquared = curR * curR;
                iSquared = curI * curI;
                iter++;
            }
            gp.output[index] = iter;
        }

        public void Dispose()
        {
            accelerator?.Dispose();
            context?.Dispose();
        }

        private void ApplySmoothing(SKBitmap set, uint passes)
        {
            for(int p = 0; p < passes; p++)
            {
                for (int i = 0; i < set.Height; i++)
                {
                    for (int j = 0; j < set.Width; j++)
                    {
                        //make this pixel the average of the 4 surrounding pixels
                        var pixel = set.GetPixel(j, i);
                        var left = j == 0 ? pixel : set.GetPixel(j - 1, i);
                        var right = j == set.Width - 1 ? pixel : set.GetPixel(j + 1, i);
                        var up = i == 0 ? pixel : set.GetPixel(j, i - 1);
                        var down = i == set.Height - 1 ? pixel : set.GetPixel(j, i + 1);

                        var avg = new SKColor(
                            (byte)((pixel.Red + left.Red + right.Red + up.Red + down.Red) / 5),
                            (byte)((pixel.Green + left.Green + right.Green + up.Green + down.Green) / 5),
                            (byte)((pixel.Blue + left.Blue + right.Blue + up.Blue + down.Blue) / 5)
                        );
                        set.SetPixel(j, i, avg);
                    }
                }
            }
        }

        private void DrawGrid(SKBitmap set)
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
