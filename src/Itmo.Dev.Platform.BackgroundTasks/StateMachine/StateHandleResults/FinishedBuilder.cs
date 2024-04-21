using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.StateHandleResults;

public readonly struct FinishedBuilder
{
    public readonly struct StateBaseBuilder
    {
        public StateValueBuilder<TStateBase> ForState<TStateBase>() where TStateBase : IState => new();
    }

    public readonly struct StateValueBuilder<TStateBase> where TStateBase : IState
    {
        public ResultBuilder<TStateBase> WithValue(TStateBase state) => new(state);
    }

    public readonly struct ResultBuilder<TStateBase> where TStateBase : IState
    {
        private readonly TStateBase _state;

        public ResultBuilder(TStateBase state)
        {
            _state = state;
        }

        public ErrorBuilder<TStateBase, TResult> ForResult<TResult>() where TResult : IBackgroundTaskResult
            => new(_state);

        public ErrorBuilder<TStateBase, EmptyExecutionResult> ForEmptyResult() => ForResult<EmptyExecutionResult>();
    }

    public readonly struct ErrorBuilder<TStateBase, TResult>
        where TStateBase : IState
        where TResult : IBackgroundTaskResult
    {
        private readonly TStateBase _state;

        public ErrorBuilder(TStateBase state)
        {
            _state = state;
        }

        public CastHandle<TStateBase, TResult, TError> ForError<TError>() where TError : IBackgroundTaskError
            => new(_state);

        public CastHandle<TStateBase, TResult, EmptyError> ForEmptyError() => ForError<EmptyError>();
    }

    public readonly struct CastHandle<TStateBase, TResult, TError>
        where TStateBase : IState 
        where TResult : IBackgroundTaskResult 
        where TError : IBackgroundTaskError
    {
        private readonly TStateBase _state;

        public CastHandle(TStateBase state)
        {
            _state = state;
        }

        public static implicit operator StateHandleResult<TStateBase, TResult, TError>(
            CastHandle<TStateBase, TResult, TError> handle)
        {
            return new StateHandleResult<TStateBase, TResult, TError>.Finished(handle._state);
        }
    }
}