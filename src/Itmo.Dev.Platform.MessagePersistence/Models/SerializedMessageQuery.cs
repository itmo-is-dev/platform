using Itmo.Dev.Platform.Common.Models;
using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.MessagePersistence.Models;

[GenerateBuilder]
public partial record SerializedMessageQuery(
    string Name,
    MessageState[] States,
    DateTimeOffset? Cursor,
    int PageSize);