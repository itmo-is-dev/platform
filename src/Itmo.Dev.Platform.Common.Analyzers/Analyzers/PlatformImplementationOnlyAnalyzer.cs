using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Itmo.Dev.Platform.Common.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlatformImplementationOnlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "IID0001",
        title: "Implementation allowed only in platform packages",
        messageFormat: "Implementation of this interface is not allowed: {0}",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeTypeDeclaration, SymbolKind.NamedType);
    }

    private static void AnalyzeTypeDeclaration(SymbolAnalysisContext context)
    {
        var symbol = (INamedTypeSymbol)context.Symbol;
        var baseTypes = symbol.Interfaces;

        if (symbol.BaseType is not null)
            baseTypes = baseTypes.Add(symbol.BaseType);

        foreach (INamedTypeSymbol baseType in baseTypes)
        {
            var attribute = HasPlatformImplementationOnlyAttribute(baseType.GetAttributes());

            if (attribute is null)
                continue;

            var argument = attribute.ConstructorArguments[0];

            var diagnostic = Diagnostic.Create(
                Descriptor,
                symbol.Locations.First(),
                argument);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static AttributeData? HasPlatformImplementationOnlyAttribute(ImmutableArray<AttributeData> attributes)
    {
        return attributes.FirstOrDefault(attributeData =>
        {
            if (attributeData.AttributeClass is null)
                return false;

            var name = attributeData.AttributeClass.Name;

            if (name != "PlatformImplementationOnlyAttribute")
                return false;

            return true;
        });
    }
}
