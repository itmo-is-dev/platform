namespace Itmo.Dev.Platform.Testing.Behavioural.Tools;

public sealed class FailedCheckException(string message)
    : Exception($"Check failed: {message}. See logs for errors.");
