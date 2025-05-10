using Itmo.Dev.Platform.Common.Exceptions;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Locking.Exceptions;

public class PlatformLockingException : PlatformException
{
    private PlatformLockingException(string message, Exception? inner) : base(message, inner) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PlatformLockingException Error(Exception? inner)
    {
        return new PlatformLockingException("Error occured while acquiring lock", inner);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PlatformLockingException FailedToAcquire()
    {
        return new PlatformLockingException("Failed to acquire lock", inner: null);
    }
}
