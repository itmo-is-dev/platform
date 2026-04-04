using Grpc.Core;

namespace Itmo.Dev.Platform.Testing.Behavioural.Extensions;

public static class GrpcExtensions
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