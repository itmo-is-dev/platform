using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Metadata;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine;

public interface IStateMachineFactory
{
    IStateMachineFactoryMetadataSelector<TStateBase> CreateForState<TStateBase>() where TStateBase : IState;
}

public interface IStateMachineFactoryMetadataSelector<TStateBase>
    where TStateBase : IState
{
    IStateMachineFactoryExecutionMetadataSelector<TStateBase, TMetadata> ForMetadata<TMetadata>()
        where TMetadata : IBackgroundTaskMetadata;

    IStateMachineFactoryExecutionMetadataSelector<TStateBase, EmptyMetadata> ForEmptyMetadata();
}

public interface IStateMachineFactoryExecutionMetadataSelector<TStateBase, TMetadata>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
{
    IStateMachineFactoryResultSelector<TStateBase, TMetadata, TExecutionMetadata> ForExecutionMetadata<
        TExecutionMetadata>() where TExecutionMetadata : IStateExecutionMetadata<TStateBase>;
}

public interface IStateMachineFactoryResultSelector<TStateBase, TMetadata, TExecutionMetadata>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
{
    IStateMachineFactoryErrorSelector<TStateBase, TMetadata, TExecutionMetadata, TResult> ForResult<TResult>()
        where TResult : IBackgroundTaskResult;

    IStateMachineFactoryErrorSelector<TStateBase, TMetadata, TExecutionMetadata, EmptyExecutionResult> ForEmptyResult();
}

public interface IStateMachineFactoryErrorSelector<
    TStateBase,
    TMetadata,
    TExecutionMetadata,
    TResult>
    where TStateBase : IState
    where TMetadata : IBackgroundTaskMetadata
    where TExecutionMetadata : IStateExecutionMetadata<TStateBase>
    where TResult : IBackgroundTaskResult
{
    IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, TError> ForError<TError>()
        where TError : IBackgroundTaskError;

    IStateMachineBuilder<TStateBase, TMetadata, TExecutionMetadata, TResult, EmptyError> ForEmptyError();
}