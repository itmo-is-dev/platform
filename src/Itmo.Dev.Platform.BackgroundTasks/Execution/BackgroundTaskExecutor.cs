using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Models;
using Itmo.Dev.Platform.BackgroundTasks.Persistence;
using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.ExecutionMetadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.BackgroundTasks.Execution;

internal class BackgroundTaskExecutor<TTask, TMetadata, TExecutionMetadata, TResult, TError> :
    IBackgroundTaskInternalExecutor
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IBackgroundTaskExecutionMetadata
    where TResult : IBackgroundTaskResult
    where TError : IBackgroundTaskError
    where TTask : IBackgroundTask<TMetadata, TExecutionMetadata, TResult, TError>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BackgroundTaskExecutionOptions _options;
    private readonly IBackgroundTaskInfrastructureRepository _repository;

    public BackgroundTaskExecutor(
        IServiceProvider serviceProvider,
        IOptions<BackgroundTaskExecutionOptions> options,
        IBackgroundTaskInfrastructureRepository repository)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _repository = repository;
    }

    public async Task ExecuteAsync(
        BackgroundTask backgroundTask,
        CancellationToken cancellationToken)
    {
        var context = new ConcreteBackgroundTaskExecutionContext<TMetadata, TExecutionMetadata>(
            backgroundTask,
            _repository,
            _options);

        var task = _serviceProvider.GetRequiredService<TTask>();

        try
        {
            var result = await task.ExecuteAsync(context, cancellationToken);

            context.HandleResult(result);
            await context.PersistAsync(cancellationToken);
        }
        catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            await _repository.UpdateAsync(backgroundTask, default);
        }
    }
}
