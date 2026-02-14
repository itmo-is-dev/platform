using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

[GenerateBuilder]
internal partial record PersistedMessageQuery(
    long[] Ids,
    string[] Names,
    MessageState[] States,
    DateTimeOffset? Cursor,
    [RequiredValue] int PageSize);
