using Itmo.Dev.Platform.Common.Tools;
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
            object? value = member.GetValue(step);

            if (ShouldWriteOptionalValue(member, ref value) is false)
                continue;

            membersWriter.Writer.WriteLine(string.Empty);
            membersWriter.Writer.Write(member.Name);
            membersWriter.Writer.Write(" = ");
            membersWriter.Writer.Write(value?.ToString() ?? "null");
        }
    }

    /// <summary>
    ///     Tries to unwrap value of Optional`1.
    /// </summary>
    /// <param name="member">
    ///     Inspected property.
    /// </param>
    /// <param name="value">
    ///     Property value when entering; when exiting – unwrapped optional value if property
    ///     type is Optional`1 and method returned true. 
    /// </param>
    /// <returns>
    ///     true – if value is not Optional`1 <br/>
    ///     true - if value is Optional`1 and HasValue <br/>
    ///     false – if value is Optional`1 and HasValue is false
    /// </returns>
    private static bool ShouldWriteOptionalValue(PropertyInfo member, ref object? value)
    {
        if (member.PropertyType.IsConstructedGenericType is false
            || member.PropertyType.GetGenericTypeDefinition() != typeof(Optional<>))
        {
            return true;
        }

        if (value is null)
            return false;

        var hasValue = member.PropertyType
            .GetProperty(nameof(Optional<>.HasValue), BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(value);

        if (hasValue is false)
            return false;

        value = member.PropertyType
            .GetProperty(nameof(Optional<>.Value), BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(value);

        return true;
    }

    private string GetName(Type stepType)
    {
        StepAttribute? stepAttribute = stepType.GetCustomAttribute<StepAttribute>();
        return stepAttribute?.Name ?? stepType.Name;
    }
}
