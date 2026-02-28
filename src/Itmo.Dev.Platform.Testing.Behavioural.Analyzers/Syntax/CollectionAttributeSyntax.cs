using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public sealed class CollectionAttributeSyntax(INamedTypeSymbol contextSymbol)
{
    public AttributeSyntax Value => Attribute(IdentifierName("Xunit.Collection"))
        .AddArgumentListArguments(
            AttributeArgument(LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(contextSymbol.Name))));
}
