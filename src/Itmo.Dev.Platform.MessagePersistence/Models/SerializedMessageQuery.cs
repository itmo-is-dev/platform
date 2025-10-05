using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.MessagePersistence.Models;

[GenerateBuilder]
internal partial record SerializedMessageQuery(
    long[] Ids,
    string[] Names,
    MessageState[] States,
    DateTimeOffset? Cursor,
    [RequiredValue] int PageSize);
