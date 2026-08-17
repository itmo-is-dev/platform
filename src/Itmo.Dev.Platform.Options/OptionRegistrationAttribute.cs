namespace Itmo.Dev.Platform.Options;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class OptionRegistrationAttribute(string sectionName, Type optionsType) : Attribute;
