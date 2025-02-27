using Itmo.Dev.Platform.BackgroundTasks.Tasks;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Errors;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;

namespace Itmo.Dev.Platform.BackgroundTasks.StateMachine.StateHandleResults;

public readonly struct FinishedWithResultBuilder
{
    public readonly struct StateBaseBuilder
    {
        public StateValueBuilder<TStateBase> ForState<TStateBase>()
            where TStateBase : IState => new();
    }

    public readonly struct StateValueBuilder<TStateBase>
        where TStateBase : IState
    {
        public ResultBuilder<TStateBase> WithValue(TStateBase state) => new(state);
    }

    public readonly struct ResultBuilder<TStateBase>
        where TStateBase : IState
    {
        private readonly TStateBase _state;

        public ResultBuilder(TStateBase state)
        {
            _state = state;
        }

        public ErrorBuilder<TStateBase, TResult> ForResult<TResult>()
            where TResult : IBackgroundTaskResult
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

        public ExecutionResultBuilder<TStateBase, TResult, TError> ForError<TError>()
            where TError : IBackgroundTaskError
        {
            return new ExecutionResultBuilder<TStateBase, TResult, TError>(_state);
        }

        public ExecutionResultBuilder<TStateBase, TResult, EmptyError> ForEmptyError() => ForError<EmptyError>();
    }

    public readonly struct ExecutionResultBuilder<TStateBase, TResult, TError>
        where TStateBase : IState
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        private readonly TStateBase _state;

        public ExecutionResultBuilder(TStateBase state)
        {
            _state = state;
        }

        public CastHandle<TStateBase, TResult, TError> WithExecutionResult(
            BackgroundTaskExecutionResult<TResult, TError> result)
        {
            return new CastHandle<TStateBase, TResult, TError>(_state, result);
        }

        public CastHandle<TStateBase, TResult, TError> WithSuccessResult(TResult result)
        {
            return new CastHandle<TStateBase, TResult, TError>(
                _state,
                new BackgroundTaskExecutionResult<TResult, TError>.Success(result));
        }

        public CastHandle<TStateBase, TResult, TError> WithSuspendedResult()
        {
            return new CastHandle<TStateBase, TResult, TError>(
                _state,
                new BackgroundTaskExecutionResult<TResult, TError>.Suspended(Until: null));
        }

        public CastHandle<TStateBase, TResult, TError> WithSuspendedUntilResult(DateTimeOffset value)
        {
            return new CastHandle<TStateBase, TResult, TError>(
                _state,
                new BackgroundTaskExecutionResult<TResult, TError>.Suspended(Until: value));
        }

        public CastHandle<TStateBase, TResult, TError> WithFailureResult(TError? error = default)
        {
            return new CastHandle<TStateBase, TResult, TError>(
                _state,
                new BackgroundTaskExecutionResult<TResult, TError>.Failure(error));
        }

        public CastHandle<TStateBase, TResult, TError> WithCancellationResult(TError? error = default)
        {
            return new CastHandle<TStateBase, TResult, TError>(
                _state,
                new BackgroundTaskExecutionResult<TResult, TError>.Cancellation(error));
        }
    }

    public readonly struct CastHandle<TStateBase, TResult, TError>
        where TStateBase : IState
        where TResult : IBackgroundTaskResult
        where TError : IBackgroundTaskError
    {
        private readonly TStateBase _state;
        private readonly BackgroundTaskExecutionResult<TResult, TError> _result;

        public CastHandle(TStateBase state, BackgroundTaskExecutionResult<TResult, TError> result)
        {
            _state = state;
            _result = result;
        }

        public static implicit operator StateHandleResult<TStateBase, TResult, TError>(
            CastHandle<TStateBase, TResult, TError> handle)
        {
            return new StateHandleResult<TStateBase, TResult, TError>.FinishedWithResult(
                handle._state,
                handle._result);
        }
    }
}
