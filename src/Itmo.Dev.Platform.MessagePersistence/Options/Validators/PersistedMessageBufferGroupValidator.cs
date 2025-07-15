using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Options.Validators;

internal class PersistedMessageBufferGroupValidator : IValidateOptions<PersistedMessageOptions>
{
    private readonly IOptionsMonitor<BufferGroupOptions> _bufferGroupOptions;

    public PersistedMessageBufferGroupValidator(IOptionsMonitor<BufferGroupOptions> bufferGroupOptions)
    {
        _bufferGroupOptions = bufferGroupOptions;
    }

    public ValidateOptionsResult Validate(string? name, PersistedMessageOptions options)
    {
        if (options.BufferGroup is not null)
        {
            if (_bufferGroupOptions.Get(options.BufferGroup).IsInitialized is false)
            {
                return ValidateOptionsResult.Fail(
                    $"Could not find buffer group '{options.BufferGroup}' but message '{name}' is configured for it");
            }
        }

        return ValidateOptionsResult.Success;
    }
}
