using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence;

public class MessageStateUpdateRequest
{
    private readonly IReadOnlyDictionary<long, MessageState> _dictionary;

    public MessageStateUpdateRequest(IReadOnlyDictionary<long, MessageState> dictionary)
    {
        _dictionary = dictionary;
    }

    public IEnumerable<long> Ids => _dictionary.Keys;
    public IEnumerable<MessageState> States => _dictionary.Values;
}