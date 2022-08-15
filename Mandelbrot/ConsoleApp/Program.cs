using Mandelbrot.Server.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddSingleton<Camera>()
            .AddSingleton<EscapeTime>()
            .AddSingleton<SetGenerator>()
    ).Build();


await host.RunAsync();

//BenchmarkRunner.Run<SetGeneratorBenchmarks>();