using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mandelbrot;

public class Program
{
    public static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Hello world!");

        Core.EscapeTime.CalcEscapeTime(new Models.BigComplex(new Models.BigDecimal(0.00001), new Models.BigDecimal(0.00002)));
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
    }
}