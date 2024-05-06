using FluentAssertions;
using Itmo.Dev.Platform.Enrichment.Extensions;
using Itmo.Dev.Platform.Enrichment.Tests.Handlers;
using Itmo.Dev.Platform.Enrichment.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Itmo.Dev.Platform.Enrichment.Tests;

public class EnrichmentProcessorTests
{
    [Fact]
    public async Task EnrichAsync_ShouldCallHandler_WhenItIsRegistered()
    {
        // Arrange
        var collection = new ServiceCollection();

        collection.AddPlatformEnrichment(
            enrichment => enrichment.Enrich<int, EnrichedModel>(
                model => model.WithHandler<AbobaModelHandler>()));

        collection.AddLogging();

        var provider = collection.BuildServiceProvider();
        var factory = provider.GetRequiredService<IEnrichmentProcessorFactory>();

        EnrichedModel[] models = [new EnrichedModel(1)];

        // Act
        models = await factory
            .Create<int, EnrichedModel>()
            .EnrichAsync(models, default)
            .ToArrayAsync();

        // Assert
        models.Should().ContainSingle().Which.Value.Should().Be(AbobaModelHandler.Value);
    }

    [Fact]
    public async Task EnrichAsync_ShouldNotProduceError_WhenNoHandlersRegistered()
    {
        // Arrange
        var collection = new ServiceCollection();

        collection.AddPlatformEnrichment(enrichment => enrichment.Enrich<int, EnrichedModel>());
        collection.AddLogging();

        var provider = collection.BuildServiceProvider();
        var factory = provider.GetRequiredService<IEnrichmentProcessorFactory>();

        EnrichedModel[] models = [new EnrichedModel(1)];

        // Act
        models = await factory
            .Create<int, EnrichedModel>()
            .EnrichAsync(models, default)
            .ToArrayAsync();

        // Assert
        models.Should().ContainSingle().Which.Value.Should().BeNull();
    }
}