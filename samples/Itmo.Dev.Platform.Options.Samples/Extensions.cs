using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Options.Samples;

public static class Extensions
{
    public static void RegisterOptions(this IServiceCollection collection)
    {
        collection.AddOptions<SomeOptions>().BindConfiguration("Application:SomeOptions");
    }
}
