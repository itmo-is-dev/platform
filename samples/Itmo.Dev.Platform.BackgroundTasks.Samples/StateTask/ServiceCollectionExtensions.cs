using FluentSerialization;
using FluentSerialization.Extensions.NewtonsoftJson;
using Itmo.Dev.Platform.BackgroundTasks.Configuration;
using Itmo.Dev.Platform.BackgroundTasks.Tasks.Results;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public static class ServiceCollectionExtensions
{
    public static void AddStateTask(this IBackgroundTaskConfigurationBuilder builder)
    {
        builder.AddBackgroundTask(b => b
            .WithMetadata<StateTaskMetadata>()
            .WithExecutionMetadata<StateTaskExecutionMetadata>()
            .WithResult<EmptyExecutionResult>()
            .WithError<StateTaskError>()
            .HandleBy<StateBackgroundTask>());
    }

    public static void ConfigureSerialization(this IServiceCollection collection)
    {
        collection.Configure<JsonSerializerSettings>(settings
            => SerializationConfigurationFactory.Build(new SerializationConfiguration()).ApplyToSerializationSettings(settings));
    }
}