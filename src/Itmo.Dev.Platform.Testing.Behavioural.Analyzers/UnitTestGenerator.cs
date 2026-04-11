using Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Extensions;
using Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;
using Itmo.Dev.Platform.Testing.Behavioural.Analyzers.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers;

[Generator]
public sealed class UnitTestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(IsTestClass, GenerateTest)
            .Where(test => test is not null)
            .Select((test, _) => test!);

        context.RegisterSourceOutput(
            syntaxProvider,
            static (context, generatedTest) => context.AddSource(
                hintName: $"{generatedTest.SourceTest.Name}.g.cs",
                source: generatedTest.GeneratedSyntax.NormalizeWhitespace(eol: "\n").ToFullString()));
    }

    private static bool IsTestClass(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
            return false;

        if (classDeclaration.BaseList is null)
            return false;

        var hasTestBaseType = classDeclaration.BaseList.Types
            .Select(type => type.Type)
            .OfType<GenericNameSyntax>()
            .Any(type => type.Identifier.Text.Contains("FeatureBase"));

        if (hasTestBaseType is false)
            return false;

        var hasTestAttribute = classDeclaration.AttributeLists
            .SelectMany(attr => attr.Attributes)
            .Select(attr => attr.Name)
            .OfType<IdentifierNameSyntax>()
            .Any(name => name.Identifier.Text.Contains("Feature"));

        if (hasTestAttribute is false)
            return false;

        return true;
    }

    private static GeneratedTest? GenerateTest(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var testBaseType = context.SemanticModel.Compilation.GetTypeByMetadataName(
            Constants.FeatureBase.MetadataFullName);

        var scenarioAttribute = context.SemanticModel.Compilation.GetTypeByMetadataName(
            Constants.ScenarioAttribute.MetadataFullName);

        if (testBaseType is null || scenarioAttribute is null)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol sourceTestSymbol)
            return null;

        if (sourceTestSymbol.BaseType is null)
            return null;

        if (sourceTestSymbol.BaseType.ConstructedFrom.Equals(testBaseType, SymbolEqualityComparer.Default) is false)
            return null;

        if (sourceTestSymbol.BaseType.TypeArguments is not [INamedTypeSymbol contextSymbol])
            return null;

        try
        {
            var node = (ClassDeclarationSyntax)context.Node;
            var sourceTestField = new SourceTestFieldSyntax(sourceTestSymbol);

            var testMethodWalker = new TestMethodSyntaxWalker(context.SemanticModel);
            var testMethodRewriter = new TestMethodSyntaxRewriter(sourceTestField.Name);

            testMethodWalker.Visit(node);

            var testMethods = testMethodWalker.Methods
                .Select(method => (MemberDeclarationSyntax)testMethodRewriter.VisitMethodDeclaration(method)!)
                .ToArray();

            var generatedNamespaceName = IdentifierName(sourceTestSymbol.ContainingNamespace.GetTypeName());
            var generatedTestName = $"{sourceTestSymbol.Name}Feature";

            var constructor = new GeneratedTestConstructorSyntax(
                sourceTestSymbol,
                contextSymbol,
                sourceTestField,
                generatedTestName);

            var generatedClassDeclaration = ClassDeclaration(generatedTestName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
                .AddAttributeLists(AttributeList().AddAttributes(new CollectionAttributeSyntax(contextSymbol).Value))
                .AddAttributeLists(AttributeList()
                    .AddAttributes(new GeneratedTestNameAttributeSyntax(sourceTestSymbol).Declaration))
                .AddBaseListTypes(SimpleBaseType(IdentifierName("Xunit.IAsyncLifetime")))
                .AddMembers(sourceTestField.Declaration, constructor.Declaration)
                .AddMembers(
                    new GeneratedTestInitializeSyntax(sourceTestField).Declaration,
                    new GeneratedTestDisposeSyntax(sourceTestField).Declaration)
                .AddMembers(testMethods)
                .AddMembers(RunnerAccessorSyntax.Declaration);

            var sourceUsings = node.SyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .ToArray();

            var ns = NamespaceDeclaration(generatedNamespaceName).AddMembers(generatedClassDeclaration);
            var unit = CompilationUnit().AddUsings(sourceUsings).AddMembers(ns);

            return new GeneratedTest(sourceTestSymbol, unit);
        }
        catch
        {
            return null;
        }
    }

    private sealed record GeneratedTest(INamedTypeSymbol SourceTest, SyntaxNode GeneratedSyntax);
}
