using Newtonsoft.Json;
using System.Text.Json;

namespace Itmo.Dev.Platform.Common.Configurations;

public static partial class PlatformCommonConfiguration
{
    public interface ISerializerStep
    {
        IFinalStep WithNewtonsoftSerialization(Action<JsonSerializerSettings>? configure = null);

        IFinalStep WithSystemTextJsonConfiguration(Action<JsonSerializerOptions>? configure = null);
    }

    public interface IFinalStep;
}
