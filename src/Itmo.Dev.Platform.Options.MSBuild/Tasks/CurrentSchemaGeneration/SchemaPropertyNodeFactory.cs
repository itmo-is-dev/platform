namespace Itmo.Dev.Platform.Options.MSBuild.Tasks.CurrentSchemaGeneration;

public static class SchemaPropertyNodeFactory
{
    public static IEnumerable<SchemaPropertyNode> FromOptionRegistrations(
        IEnumerable<OptionRegistration> registrations)
    {
        var properties = new Dictionary<string, SchemaPropertyNode>();

        foreach (OptionRegistration registration in registrations)
        {
            var pathParts = registration.Section.Split(
                separator: ":",
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            SchemaPropertyNode? currentProperty = null;

            foreach (string pathPart in pathParts)
            {
                if (currentProperty is null)
                {
                    if (properties.TryGetValue(pathPart, out currentProperty) is false)
                        currentProperty = properties[pathPart] = new SchemaPropertyNode(pathPart);
                }
                else
                {
                    currentProperty = currentProperty.GetChildProperty(pathPart);
                }
            }

            currentProperty?.SchemaTypes.Add(registration.Type);
        }

        return properties.Values;
    }
}
