using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers;

[Generator]
public sealed class CollectionFixtureGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat SymbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var testContextType = context.CompilationProvider
            .Select(static (compilation, _) => compilation
                .GetTypeByMetadataName("Itmo.Dev.Platform.Testing.Behavioural.ITestContext"));

        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(IsTestContext, MapTestContextDetails);

        var testContextTypes = syntaxProvider
            .Combine(testContextType)
            .Select(static (tuple, _) => (details: tuple.Left, testContextType: tuple.Right))
            .Where(static tuple =>
            {
                return tuple.details.TestContextSymbol.AllInterfaces.Any(i
                    => i.Equals(tuple.testContextType, SymbolEqualityComparer.Default));
            })
            .Select((tuple, _) => tuple.details.TestContextSymbol);

        context.RegisterSourceOutput(
            testContextTypes,
            static (context, testContextType) =>
            {
                var fixtureName = $"{testContextType.Name}CollectionFixture";
                var typeFullName = testContextType.ToDisplayString(SymbolDisplayFormat);
                var namespaceFullName = testContextType.ContainingNamespace.ToDisplayString(SymbolDisplayFormat);

                var attribute = Attribute(
                    IdentifierName("Xunit.CollectionDefinition"),
                    AttributeArgumentList()
                        .AddArguments(AttributeArgument(LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(testContextType.Name)))));

                var baseType = GenericName(
                    Identifier("Xunit.ICollectionFixture"),
                    TypeArgumentList().AddArguments(IdentifierName(typeFullName)));

                var cls = ClassDeclaration(fixtureName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAttributeLists(AttributeList().AddAttributes(attribute))
                    .AddBaseListTypes(SimpleBaseType(baseType));

                var ns = NamespaceDeclaration(IdentifierName(namespaceFullName)).AddMembers(cls);
                var unit = CompilationUnit().AddMembers(ns);

                context.AddSource(
                    $"{fixtureName}.g.cs",
                    unit.NormalizeWhitespace(eol: "\n").ToFullString());
            });
    }

    private static bool IsTestContext(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
            return false;

        if (classDeclaration.BaseList is null)
            return false;

        return classDeclaration.BaseList.Types
            .Select(x => x.Type)
            .OfType<IdentifierNameSyntax>()
            .Any(type => type.Identifier.Text.Contains("TestContext"));
    }

    private static TestContextDetails MapTestContextDetails(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
        return new TestContextDetails((INamedTypeSymbol)symbol!);
    }

    private readonly record struct TestContextDetails(INamedTypeSymbol TestContextSymbol);
}
