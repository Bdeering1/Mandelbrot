using BenchmarkDotNet.Attributes;
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

        [Benchmark]
        public async Task GetBitmap()
        {
            for (int i = 0; i < 100; i++)
            {
                await generator.GetBitmap();
            }
        }
    }
}

    