using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options.Validators;

internal sealed class MessagePersistenceOptionsValidator : IValidateOptions<MessagePersistenceOptions>
{
    private readonly IOptionsMonitor<PersistedMessageOptions> _persistedMessageOptions;

    public MessagePersistenceOptionsValidator(IOptionsMonitor<PersistedMessageOptions> persistedMessageOptions)
    {
        _persistedMessageOptions = persistedMessageOptions;
    }

    public ValidateOptionsResult Validate(string? name, MessagePersistenceOptions options)
    {
        foreach (var (messageType, messageName) in options.MessageNames)
        {
            var messageOptions = _persistedMessageOptions.Get(messageName);

            if (messageOptions.IsInitialized is false)
                return ValidateOptionsResult.Fail($"Persisted message called '{messageName} is not initialized'");

            if (messageOptions.MessageType != messageType)
            {
                return ValidateOptionsResult.Fail(
                    $"Persisted message called '{messageName}' has inconsistent handler type, expected: {messageType}, actual: {messageOptions.MessageType}");
            }
        }
        
        return ValidateOptionsResult.Success;
    }
}
