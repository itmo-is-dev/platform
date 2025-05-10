using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Locking.FormattingStrategies;

internal class LockingKeyFormatterProvider : ILockingKeyFormatterProvider
{
    private readonly IServiceProvider _serviceProvider;

    public LockingKeyFormatterProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILockingKeyFormatter GetFormatter(object key)
    {
        return _serviceProvider.GetKeyedService<ILockingKeyFormatter>(key.GetType())
               ?? _serviceProvider.GetRequiredService<ILockingKeyFormatter>();
    }
}
