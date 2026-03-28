namespace Itmo.Dev.Platform.Testing.Behavioural.Tools;

public sealed class FailedCheckException(string message, Exception? lastException)
    : Exception($"Check failed: {message}. See logs for errors.", lastException);
