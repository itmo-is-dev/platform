using Microsoft.Extensions.Options;
using Moq;

namespace Itmo.Dev.Platform.Testing;

public static class OptionsMonitorExtensions
{
    public static IOptionsMonitor<T> AsOptionsMonitor<T>(this T value)
    {
        var mock = new Mock<IOptionsMonitor<T>>();
        mock.Setup(x => x.CurrentValue).Returns(value);

        return mock.Object;
    }
}