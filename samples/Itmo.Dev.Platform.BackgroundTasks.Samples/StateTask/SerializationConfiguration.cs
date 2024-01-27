using FluentSerialization;
using Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask.States;

namespace Itmo.Dev.Platform.BackgroundTasks.Samples.StateTask;

public class SerializationConfiguration : ISerializationConfiguration
{
    public void Configure(ISerializationConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Type<StartingState>().HasTypeKey("Starting");
        configurationBuilder.Type<FirstState>().HasTypeKey("First");
        configurationBuilder.Type<CompletedState>().HasTypeKey("Completed");
    }
}