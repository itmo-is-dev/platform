using Microsoft.Extensions.Hosting;

namespace Itmo.Dev.Platform.Common.BackgroundServices;

public abstract class RestartableBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested is false)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            try
            {
                await ExecuteAsync(cts);
            }
            catch (Exception e)
                when (e is OperationCanceledException or TaskCanceledException && cts.IsCancellationRequested) { }
        }
    }

    protected abstract Task ExecuteAsync(CancellationTokenSource cts);
}