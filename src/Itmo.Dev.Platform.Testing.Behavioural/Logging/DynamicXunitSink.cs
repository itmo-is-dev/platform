using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.XUnit;
using Xunit.Abstractions;

namespace Itmo.Dev.Platform.Testing.Behavioural.Logging;

public sealed class DynamicXunitSink : ILogEventSink
{
    private static readonly ITextFormatter TextFormatter = new MessageTemplateTextFormatter(
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

    private ILogEventSink? _innerSink;

    public void Emit(LogEvent logEvent)
    {
        _innerSink?.Emit(logEvent);
    }

    public void SetTestOutput(ITestOutputHelper output)
    {
        _innerSink = new TestOutputSink(output, TextFormatter);
    }

    public void ClearTestOutput()
    {
        _innerSink = null;
    }
}
