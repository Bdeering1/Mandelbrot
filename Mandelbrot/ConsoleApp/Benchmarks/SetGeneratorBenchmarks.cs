using BenchmarkDotNet.Attributes;
using Mandelbrot.Server.Core;

namespace Mandelbrot.Console.Benchmarks
{
    [MemoryDiagnoser]
    //[Orderer(SummaryOrderPolicy.FastestToSlowest)]
    //[RankColumn]
    public class SetGeneratorBenchmarks
    {
        public SetGenerator generator { get; }

        public SetGeneratorBenchmarks(SetGenerator generator)
        {
            this.generator = generator;
        }


        [Benchmark]
        public async Task GetBitmap()
        {
            await generator.GetBitmap();
        }
    }
}

