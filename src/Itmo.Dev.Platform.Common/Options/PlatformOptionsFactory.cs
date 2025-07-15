using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Common.Options;

public class PlatformOptionsFactory<T> : IOptionsFactory<T>
    where T : class
{
    private readonly OptionsFactory<T> _factory;

    public PlatformOptionsFactory(IServiceProvider provider)
    {
        _factory = ActivatorUtilities.CreateInstance<OptionsFactory<T>>(provider);
    }

    public T Create(string name)
    {
        try
        {
            return _factory.Create(name);
        }
        catch (OptionsValidationException e)
        {
            throw new PlatformOptionsValidationException(e);
        }
    }
}
