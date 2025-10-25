using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Observability.Tests.Startup.Tools;

public class ParentSpanInterceptor : Interceptor
{
    public static string? Span { get; private set; }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var header = context.RequestHeaders.Single(x => x.Key == "traceparent");
        Span = header.Value.Split('-')[2];
        Console.WriteLine($"Span: {Span}");

        await File.WriteAllTextAsync(
            Environment.GetEnvironmentVariable(TestingConstants.SpanIdFileVariableName) ?? string.Empty,
            Span);

        return await continuation(request, context);
    }
}
