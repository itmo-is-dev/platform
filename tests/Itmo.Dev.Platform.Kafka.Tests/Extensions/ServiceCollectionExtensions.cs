using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Itmo.Dev.Platform.Kafka.Tests.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectAsOptions<T>(this IServiceCollection collection, T obj)
        where T : class
    {
        var options = new Mock<IOptions<T>>();
        var optionsSnapshot = new Mock<IOptionsSnapshot<T>>();
        var optionsMonitor = new Mock<IOptionsMonitor<T>>();

        options.Setup(x => x.Value).Returns(obj);
        optionsSnapshot.Setup(x => x.Value).Returns(obj);
        optionsMonitor.Setup(x => x.CurrentValue).Returns(obj);

        collection.AddSingleton(options.Object);
        collection.AddSingleton(optionsSnapshot.Object);
        collection.AddSingleton(optionsMonitor.Object);

        return collection;
    }
}