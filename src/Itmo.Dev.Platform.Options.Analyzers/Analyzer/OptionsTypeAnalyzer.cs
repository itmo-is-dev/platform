using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceKit;

namespace Itmo.Dev.Platform.Options.Analyzers.Analyzer;

[Generator]
public sealed class OptionsTypeAnalyzer : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor IncorrectOptionsTypeAccessibilityDescriptor = new(
        id: "IID1001",
        title: "Incorrect options type accessibility",
        messageFormat: "Types, marked with [OptionsType] attribute must be public",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Itmo.Dev.Platform.Options.OptionsTypeAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (context, _) => IncrementalResult.Failure([.. EnumerateTypeSymbolDiagnostics(context)]))
            .Unwrap(context);
    }

    private static IEnumerable<Diagnostic> EnumerateTypeSymbolDiagnostics(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            yield break;

        if (typeSymbol.DeclaredAccessibility is not Accessibility.Public)
        {
            yield return Diagnostic.Create(
                IncorrectOptionsTypeAccessibilityDescriptor,
                context.TargetNode.GetLocation());
        }
    }
}
