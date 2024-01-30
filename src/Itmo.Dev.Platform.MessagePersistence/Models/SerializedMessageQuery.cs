using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.MessagePersistence.Models;

[GenerateBuilder]
internal partial record SerializedMessageQuery(
    string Name,
    MessageState[] States,
    DateTimeOffset? Cursor,
    int PageSize);