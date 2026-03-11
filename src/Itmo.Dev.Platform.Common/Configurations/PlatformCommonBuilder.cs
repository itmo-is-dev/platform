using Itmo.Dev.Platform.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json;
using static Itmo.Dev.Platform.Common.Configurations.PlatformCommonConfiguration;

namespace Itmo.Dev.Platform.Common.Configurations;

internal sealed class PlatformCommonBuilder(IServiceCollection collection) : ISerializerStep, IFinalStep
{
    public IFinalStep WithNewtonsoftSerialization(Action<JsonSerializerSettings>? configure = null)
    {
        collection.AddSingleton<IPlatformSerializer, NewtonsoftSerializer>();
        collection.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);
        
        var optionsBuilder = collection.AddOptions<JsonSerializerSettings>();

        if (configure is not null)
            optionsBuilder.Configure(configure);

        return this;
    }

    public IFinalStep WithSystemTextJsonConfiguration(Action<JsonSerializerOptions>? configure = null)
    {
        collection.AddSingleton<IPlatformSerializer, SystemTextJsonSerializer>();
        collection.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerOptions>>().Value);

        var optionsBuilder = collection.AddOptions<JsonSerializerOptions>();

        if (configure is not null)
            optionsBuilder.Configure(configure);
        
        return this;
    }
}
