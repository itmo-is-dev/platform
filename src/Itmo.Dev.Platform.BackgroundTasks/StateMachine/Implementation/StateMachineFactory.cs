using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.Implementation;

internal class StateMachineFactory : IStateMachineFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StateMachineFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStateMachineFactoryMetadataSelector<TStateBase> CreateForState<TStateBase>()
        where TStateBase : IState
        => new MetadataSelector<TStateBase>(_serviceProvider);
}

file class MetadataSelector<TStateBase> : IStateMachineFactoryMetadataSelector<TStateBase>
    where TStateBase : IState
{
    private readonly IServiceProvider _serviceProvider;

    public MetadataSelector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStateMachineFactoryExecutionMetadataSelector<TStateBase, TMetadata> ForMetadata<TMetadata>()
        where TMetadata : IBackgroundTaskMetadata
    {
        return new ExecutionMetadataSelector<TStateBase, TMetadata>(_serviceProvider);
    }

    public IStateMachineFactoryExecutionMetadataSelector<TStateBase, EmptyMetadata> ForEmptyMetadata()
        => ForMetadata<EmptyMetadata>();
}

file class ExecutionMetadataSelector<TStateBase, TMetadata> : IStateMachineFactoryExecutionMetadataSelector<
    TStateBase,
    TMetadata>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutionMetadataSelector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStateMachineFactoryResultSelector<TStateBase, TMetadata, TExecutionMetadata> ForExecutionMetadata<
        TExecutionMetadata>()
        where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    {
        return new ResultSelector<TStateBase, TMetadata, TExecutionMetadata>(_serviceProvider);
    }
}

file class ResultSelector<TStateBase, TMetadata, TExecutionMetadata> : IStateMachineFactoryResultSelector<
    TStateBase,
    TMetadata,
    TExecutionMetadata>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
{
    private readonly IServiceProvider _serviceProvider;

    public ResultSelector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStateMachineFactoryErrorSelector<TStateBase, TMetadata, TExecutionMetadata, TResult> ForResult<TResult>()
        where TResult : IBackgroundTaskResult
    {
        return new ErrorSelector<TStateBase, TMetadata, TExecutionMetadata, TResult>(_serviceProvider);
    }

    public IStateMachineFactoryErrorSelector<TStateBase, TMetadata, TExecutionMetadata, EmptyExecutionResult>
        ForEmptyResult()
    {
        return ForResult<EmptyExecutionResult>();
    }
}

file class ErrorSelector<TStateBase, TMetadata, TExecutionMetadata, TResult> : IStateMachineFactoryErrorSelector<
    TStateBase,
    TMetadata,
    TExecutionMetadata,
    TResult>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
{
    private readonly IServiceProvider _serviceProvider;

    public ErrorSelector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> ForError<TError>()
        where TError : IBackgroundTaskError
    {
        return new StateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError>(_serviceProvider);
    }

    public IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, EmptyError> ForEmptyError()
    {
        return ForError<EmptyError>();
    }
}