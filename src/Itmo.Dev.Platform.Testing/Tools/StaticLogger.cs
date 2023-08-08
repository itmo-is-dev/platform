using Serilog;
using Serilog.Events;

namespace Itmo.Dev.Platform.Testing.Tools;

public class StaticLogger : ILogger
{
    public void Write(LogEvent logEvent)
    {
        Log.Logger.Write(logEvent);
    }
}