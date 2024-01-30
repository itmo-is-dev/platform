using Xunit;

namespace Itmo.Dev.Platform.MessagePersistence.Tests.Fixtures;

[CollectionDefinition(nameof(MessagePersistenceCollectionFixture))]
public class MessagePersistenceCollectionFixture : ICollectionFixture<MessagePersistenceDatabaseFixture>;