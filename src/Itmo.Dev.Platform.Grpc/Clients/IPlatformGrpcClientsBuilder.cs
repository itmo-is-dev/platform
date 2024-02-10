namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsBuilder AddHeaderProvider<TProvider>() where TProvider : class, IPlatformGrpcHeaderProvider;

    IPlatformGrpcClientsBuilder AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action);
}