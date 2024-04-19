namespace Itmo.Dev.Platform.Common.Features;

internal interface IPlatformFeature
{
    static abstract string Name { get; }

    static abstract string RegistrationMethod { get; }
}