using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public sealed class GeneratedTestNameAttributeSyntax
{
    private readonly string _featureName;

    public GeneratedTestNameAttributeSyntax(INamedTypeSymbol sourceTestSymbol)
    {
        _featureName = sourceTestSymbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.MetadataName == Constants.FeatureAttribute.MetadataName)
            .Select(attr => attr.ConstructorArguments.First().Value)
            .Cast<string>()
            .First();
    }

    public AttributeSyntax Declaration =>
        Attribute(IdentifierName("Xunit.Trait"))
            .AddArgumentListArguments(
                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Category"))),
                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(_featureName))));
}
