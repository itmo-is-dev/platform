namespace Itmo.Dev.Platform.Observability.Tests.Startup;

public static class TestingConstants
{
    public const string IsGrpcServiceEnvVariableName = "ITMO_PLATFORM_IS_GRPC_SERVICE";
    public const string SpanIdFileVariableName = "ITMO_PLATFORM_SPAN_FILE";
    
    public const string GrpcServiceUrl = "http://127.0.0.1:8080";
    public const string HttpServiceUrl = "http://127.0.0.1:8081";
}
