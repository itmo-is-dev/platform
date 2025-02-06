using FluentAssertions;
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
        const string expectedValue = "value";

        var collection = new ServiceCollection();

        collection.AddScoped(_ => new EnrichedModelHandler(expectedValue));

        collection.AddPlatformEnrichment(
            enrichment => enrichment.Enrich<int, EnrichedModel>(
                model => model.WithHandler<EnrichedModelHandler>()));

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
        models.Should().ContainSingle().Which.Value.Should().Be(expectedValue);
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

    [Fact]
    public async Task EnrichAsync_ShouldEnrichTransitiveModels()
    {
        // Arrange
        const string expectedInner = "inner_value";
        const string expectedOuter = "outer_value";

        var collection = new ServiceCollection();

        collection.AddScoped(_ => new EnrichedModelHandler(expectedInner));
        collection.AddScoped(_ => new WrappingEnrichedModelHandler(expectedOuter));

        collection.AddPlatformEnrichment(enrichment => enrichment
            .Enrich<int, EnrichedModel>(model => model
                .WithHandler<EnrichedModelHandler>())
            .Enrich<int, WrappingEnrichedModel>(model => model
                .WithHandler<WrappingEnrichedModelHandler>()
                .WithTransitive<int, EnrichedModel>(x => x.InnerModel)));

        collection.AddLogging();

        var provider = collection.BuildServiceProvider();
        var factory = provider.GetRequiredService<IEnrichmentProcessorFactory>();

        WrappingEnrichedModel[] models = [new WrappingEnrichedModel(1, new EnrichedModel(2))];

        // Act
        models = await factory
            .Create<int, WrappingEnrichedModel>()
            .EnrichAsync(models, default)
            .ToArrayAsync();

        // Assert
        var model = models.Should().ContainSingle().Subject;
        model.OuterValue.Should().Be(expectedOuter);
        model.InnerModel.Value.Should().Be(expectedInner);
    }
}
