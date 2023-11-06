using Xunit;

namespace Itmo.Dev.Platform.BackgroundTasks.Tests.Fixtures;

[CollectionDefinition(nameof(BackgroundTasksCollectionFixture))]
public class BackgroundTasksCollectionFixture : ICollectionFixture<BackgroundTasksDatabaseFixture> { }