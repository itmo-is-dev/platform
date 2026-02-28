using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.SyntaxVisitors;

public sealed class TestMethodSyntaxWalker(SemanticModel semanticModel)
    : CSharpSyntaxWalker
{
    private readonly INamedTypeSymbol _scenarioAttributeType = semanticModel.Compilation
        .GetTypeByMetadataName(Constants.ScenarioAttribute.MetadataFullName)!;

    private readonly List<MethodDeclarationSyntax> _methods = [];

    public IReadOnlyCollection<MethodDeclarationSyntax> Methods => _methods;

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (semanticModel.GetDeclaredSymbol(node) is not { } methodSymbol)
            return;

        var isScenarioMethod = methodSymbol.GetAttributes()
            .Any(attr =>
                attr.AttributeClass is not null
                && attr.AttributeClass.Equals(_scenarioAttributeType, SymbolEqualityComparer.Default));

        if (isScenarioMethod)
        {
            _methods.Add(node);
        }
    }
}
