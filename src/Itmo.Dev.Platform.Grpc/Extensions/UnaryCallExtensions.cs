using Grpc.Core;

namespace Itmo.Dev.Platform.Grpc.Extensions;

public static class UnaryCallExtensions
{
    public static async Task<GrpcCallResult<T>> AsResultAsync<T>(this AsyncUnaryCall<T> call)
    {
        try
        {
            T value = await call;
            return new GrpcCallResult<T>.Success(value);
        }
        catch (RpcException e)
        {
            return new GrpcCallResult<T>.Failure(e);
        }
    }
}

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
