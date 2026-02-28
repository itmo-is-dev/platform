using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.SyntaxVisitors;

public sealed class TestMethodSyntaxRewriter(SyntaxToken testFieldName) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var linePragma = LineDirectiveTrivia(
            Literal(node.Identifier.GetLocation().GetMappedLineSpan().StartLinePosition.Line + 1),
            Literal(node.GetLocation().GetMappedLineSpan().Path),
            true);

        var testMethod = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName(testFieldName),
            IdentifierName(node.Identifier));

        var arguments = node.ParameterList.Parameters
            .Select(parameter => Argument(IdentifierName(parameter.Identifier)))
            .ToArray();

        var testMethodInvocation = InvocationExpression(testMethod, ArgumentList().AddArguments(arguments));

        node = node.WithBody(Block(ExpressionStatement(AwaitExpression(testMethodInvocation))));
        node = node.WithModifiers(TokenList(
            Token(SyntaxKind.PublicKeyword).WithLeadingTrivia(TriviaList().Add(Trivia(linePragma))),
            Token(SyntaxKind.AsyncKeyword)));

        return base.VisitMethodDeclaration(node);
    }

    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        if (node.Name is not IdentifierNameSyntax attributeType)
            return base.VisitAttribute(node);

        if (attributeType.Identifier.Text == Constants.ScenarioAttribute.AttributeName
            || attributeType.Identifier.Text == Constants.ScenarioAttribute.MetadataName)
        {
            var isTheoryMethod = node
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .Select(method => method.ParameterList.Parameters is not [])
                .First();

            node = node.WithName(IdentifierName(isTheoryMethod ? "Xunit.Theory" : "Xunit.Fact"));
        }

        if (attributeType.Identifier.Text == Constants.ScenarioSampleAttribute.AttributeName
            || attributeType.Identifier.Text == Constants.ScenarioSampleAttribute.MetadataName)
        {
            node = node.WithName(IdentifierName("Xunit.InlineData"));
        }

        return base.VisitAttribute(node);
    }
}
