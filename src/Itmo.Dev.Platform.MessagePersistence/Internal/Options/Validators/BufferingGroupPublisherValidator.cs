using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options.Validators;

internal class BufferingGroupPublisherValidator : IValidateOptions<BufferGroupOptions>
{
    private readonly IOptionsMonitor<MessagePersistencePublisherOptions> _publisherOptions;

    public BufferingGroupPublisherValidator(IOptionsMonitor<MessagePersistencePublisherOptions> publisherOptions)
    {
        _publisherOptions = publisherOptions;
    }

    public ValidateOptionsResult Validate(string? name, BufferGroupOptions options)
    {
        return _publisherOptions.Get(name).IsInitialized
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Could not find publisher for buffer group");
    }
}
