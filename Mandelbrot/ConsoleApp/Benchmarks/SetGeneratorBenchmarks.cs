﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Mandelbrot.Server.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Mandelbrot.ConsoleApp.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class SetGeneratorBenchmarks
    {
        public SetGenerator generator { get; }

        public SetGeneratorBenchmarks()
        {
            var services = new ServiceCollection();
            services.AddSingleton<Camera>();
            services.AddSingleton<EscapeTime>();
            services.AddSingleton<SetGenerator>();
            var container = services.BuildServiceProvider();

            generator = container.GetService<SetGenerator>();
        }

        [IterationSetup]
        public void ResetGenerator()
        {
            Console.WriteLine("Resetting generator");
            generator.Reset();
        }

        [Benchmark]
        public async Task GetBitmap()
        {
            await generator.GetBitmap();
        }
    }
}

    