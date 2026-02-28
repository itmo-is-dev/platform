using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Extensions;

public static class TypeSymbolExtensions
{
    public static string GetTypeName(this INamespaceOrTypeSymbol symbol)
        => symbol.ToDisplayString(Constants.SymbolDisplayFormat);

    public static TypeSyntax GetTypeIdentifierName(this INamespaceOrTypeSymbol symbol)
        => IdentifierName(symbol.GetTypeName());
}
