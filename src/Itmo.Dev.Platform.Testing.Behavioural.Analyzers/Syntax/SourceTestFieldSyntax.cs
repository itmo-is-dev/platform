using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public sealed class SourceTestFieldSyntax
{
    public SourceTestFieldSyntax(INamedTypeSymbol sourceTestSymbol)
    {
        Type = IdentifierName(sourceTestSymbol.ToDisplayString(Constants.SymbolDisplayFormat));
        Name = Identifier("_test");
    }

    public TypeSyntax Type { get; }

    public SyntaxToken Name { get; }

    public FieldDeclarationSyntax Declaration =>
        FieldDeclaration(VariableDeclaration(Type).AddVariables(VariableDeclarator(Name)))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword));

    public ObjectCreationExpressionSyntax Create(params ArgumentSyntax[] arguments)
        => ObjectCreationExpression(Type).AddArgumentListArguments(arguments);
}
