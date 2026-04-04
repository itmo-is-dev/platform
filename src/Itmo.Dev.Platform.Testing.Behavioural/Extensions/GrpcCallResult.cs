using Grpc.Core;

namespace Itmo.Dev.Platform.Testing.Behavioural.Extensions;

public abstract record GrpcCallResult<T>
{
    private GrpcCallResult() { }

    public sealed record Success(T Value) : GrpcCallResult<T>;

    public sealed record Failure(RpcException Exception) : GrpcCallResult<T>
    {
        public StatusCode StatusCode => Exception.StatusCode;

        public string Detail => Exception.Status.Detail;
    }
}
