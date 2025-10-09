// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

public static class TaskExtensions
{
    public static async Task<T> WrapCancellationWithMessage<T>(this Task<T> task, string message)
    {
        try
        {
            return await task;
        }
        catch (OperationCanceledException e)
        {
            throw new OperationCanceledException(message, e);
        }
    }
}
