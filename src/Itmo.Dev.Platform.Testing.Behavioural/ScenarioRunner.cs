using Itmo.Dev.Platform.Testing.Behavioural.Formatting;
using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Itmo.Dev.Platform.Testing.Behavioural.Text;
using Serilog;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Testing.Behavioural;

public sealed class ScenarioRunner<TContext>(
    TContext context,
    ITestOutputHelper outputHelper)
    : IScenarioRunner<TContext>
    where TContext : ITestContext
{
    private readonly IStepFormatter<TContext> _stepFormatter = new ReflectionStepFormatter<TContext>();

    public async Task ExecuteStep(IFeatureStep<TContext> step)
    {
        var writer = new IndentedTextWriter();
        writer.Write("Executing ");
        _stepFormatter.Format(step, writer);

        outputHelper.WriteLine(writer.ToString());

        try
        {
            long start = Stopwatch.GetTimestamp();

            await step.ExecuteAsync(context, default);

            outputHelper.WriteLine($"Execution successful ({Stopwatch.GetElapsedTime(start)})");
            outputHelper.WriteLine(string.Empty);
        }
        catch (Exception e)
        {
            outputHelper.WriteLine(
                $"Execution failed with exception of type {e.GetType().Name}, message = {e.Message}");

            throw;
        }
    }

    public async Task InitializeAsync()
    {
        await context.OnScenarioStartingAsync();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(outputHelper)
            .CreateLogger();
    }

    public async Task DisposeAsync()
    {
        await context.OnScenarioFinishedAsync();

        await Log.CloseAndFlushAsync();
    }
}
