using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Options.Analyzers.Generators;

public readonly record struct OptionRegistration(
    SyntaxToken Section,
    IdentifierNameSyntax Type)
{
    private const string AttributeName = "OptionRegistrationAttribute";
    private const string AttributeMetadataName = $"global::Itmo.Dev.Platform.Options.{AttributeName}";

    public AttributeSyntax AttributeSyntax => Attribute(IdentifierName(AttributeMetadataName))
        .AddArgumentListArguments(
            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Section)),
            AttributeArgument(TypeOfExpression(Type)));
}
