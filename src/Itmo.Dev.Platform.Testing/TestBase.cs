using Bogus;
using Itmo.Dev.Platform.Testing.Tools;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Testing;

public class TestBase
{
    public TestBase(ITestOutputHelper? output = null, LogEventLevel? minimumLevel = null)
    {
        Randomizer.Seed = new Random(Seed);
        Faker = new Faker();

        if (output is not null)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel ?? LogEventLevel.Verbose)
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }
    }

    public Faker Faker { get; }

    public static int Seed { get; set; } =
        Assembly.GetExecutingAssembly().GetCustomAttribute<SeedAttribute>()?.Value ?? 101;
}