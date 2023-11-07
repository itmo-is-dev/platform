namespace Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;

public class EmptyError : IBackgroundTaskError
{
    private EmptyError() { }

    public static readonly EmptyError Value = new EmptyError();
}