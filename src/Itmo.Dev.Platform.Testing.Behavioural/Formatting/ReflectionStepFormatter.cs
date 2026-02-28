using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Itmo.Dev.Platform.Testing.Behavioural.Text;
using System.Reflection;

namespace Itmo.Dev.Platform.Testing.Behavioural.Formatting;

public sealed class ReflectionStepFormatter<TContext> : IStepFormatter<TContext>
    where TContext : ITestContext
{
    public void Format(IFeatureStep<TContext> step, ITextWriter writer)
    {
        Type stepType = step.GetType();

        writer.Write(GetName(stepType));

        PropertyInfo[] members = stepType
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .OfType<PropertyInfo>()
            .ToArray();

        if (members.Length is 0)
            return;

        using ITextWriterScope membersWriter = writer.Indent();

        foreach (PropertyInfo member in members)
        {
            membersWriter.Writer.WriteLine(string.Empty);
            membersWriter.Writer.Write(member.Name);
            membersWriter.Writer.Write(" = ");
            membersWriter.Writer.Write(member.GetValue(step)?.ToString() ?? "null");
        }
    }

    private string GetName(Type stepType)
    {
        StepAttribute? stepAttribute = stepType.GetCustomAttribute<StepAttribute>();
        return stepAttribute?.Name ?? stepType.Name;
    }
}
