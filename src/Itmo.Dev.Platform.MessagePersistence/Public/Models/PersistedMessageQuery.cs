using SourceKit.Generators.Builder.Annotations;

namespace Itmo.Dev.Platform.MessagePersistence;

[GenerateBuilder]
public sealed partial record PersistedMessageQuery(DateTimeOffset? MinCreatedAt);
