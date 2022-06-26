using ClientProj;
using Mandelbrot.Core;
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

        //var point = new BigComplex((BigDecimal)0.350511, (BigDecimal)0.350511);
        //var escapeTime = EscapeTime.CalcEscapeTime(point);
        //Console.WriteLine($"Point: {point} Escape time: {escapeTime}");
        ConsoleTests.Colors();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
    }
}