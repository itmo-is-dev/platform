namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options;

internal sealed class MessagePersistenceOptions
{
    private readonly Dictionary<Type, string> _messageNames = [];

    public IReadOnlyDictionary<Type, string> MessageNames => _messageNames;

    public void AddPersistedMessage(Type type, string messageName)
        => _messageNames.Add(type, messageName);
}
