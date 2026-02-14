using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options;

internal class BufferGroupOptions
{
    internal bool IsInitialized { get; set; }

    [NotNull]
    [Required]
    internal string? Name { get; set; }

    [ValidateEnumeratedItems]
    public List<BufferStepOptions> Steps { get; set; } = [];

    public string? FindNextStepName(string? currentStepName)
    {
        if (currentStepName is null)
            return Steps.FirstOrDefault()?.Name;

        var nextStep = Steps
            .SkipWhile(step => step.Name.Equals(currentStepName, StringComparison.OrdinalIgnoreCase) is false)
            .Skip(1)
            .FirstOrDefault();

        return nextStep?.Name;
    }

    public BufferStepOptions? FindStep(string stepName)
        => Steps.SingleOrDefault(x => x.Name.Equals(stepName, StringComparison.OrdinalIgnoreCase));
}
