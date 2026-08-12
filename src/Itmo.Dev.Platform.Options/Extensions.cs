using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Options;

public static class Extensions
{
    public static void AddOptions(this IServiceCollection collection)
    {
        collection.AddOptions<SampleOptions>().BindConfiguration("Application:SomeSection");
    }
}
