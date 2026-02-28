using Microsoft.CodeAnalysis;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers;

public static class Constants
{
    public const string Namespace = "Itmo.Dev.Platform.Testing.Behavioural";

    public static readonly SymbolDisplayFormat SymbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    public static class ITestContext
    {
        public const string MetadataName = "ITestContext";

        public const string MetadataFullName = $"{Namespace}.{MetadataName}";
    }

    public static class FeatureBase
    {
        public const string Name = "FeatureBase";

        public const string FullName = $"{Namespace}.{Name}";

        public const string MetadataName = $"{Name}`1";

        public const string MetadataFullName = $"{Namespace}.{MetadataName}";
    }

    public static class IScenarioRunner
    {
        public const string Name = "IScenarioRunner";

        public const string FullName = $"{Namespace}.{Name}";
    }

    public static class ScenarioRunner
    {
        public const string Name = "ScenarioRunner";

        public const string FullName = $"{Namespace}.{Name}";
    }

    public static class ScenarioAttribute
    {
        public const string AttributeName = "Scenario";

        public const string MetadataName = "ScenarioAttribute";

        public const string MetadataFullName = $"{Namespace}.{MetadataName}";
    }

    public static class ScenarioSampleAttribute
    {
        public const string AttributeName = "ScenarioSample";

        public const string MetadataName = "ScenarioSampleAttribute";

        public const string MetadataFullName = $"{Namespace}.{MetadataName}";
    }

    public static class FeatureAttribute
    {
        public const string MetadataName = "FeatureAttribute";

        public const string MetadataFullName = $"{Namespace}.{MetadataName}";
    }
}
