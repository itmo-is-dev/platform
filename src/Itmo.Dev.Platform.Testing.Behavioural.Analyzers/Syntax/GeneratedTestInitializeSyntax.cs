using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public sealed class GeneratedTestInitializeSyntax(SourceTestFieldSyntax sourceTestField)
{
    private const string VariableName = "lifetime";
    private const string MethodName = "InitializeAsync";

    public MethodDeclarationSyntax Declaration =>
        MethodDeclaration(IdentifierName("Task"), Identifier(MethodName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
            .AddBodyStatements(
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("Xunit.IAsyncLifetime"))
                        .AddVariables(VariableDeclarator(VariableName)
                            .WithInitializer(
                                EqualsValueClause(IdentifierName(sourceTestField.Name))))
                ),
                ExpressionStatement(AwaitExpression(InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(VariableName),
                        IdentifierName(MethodName))
                )))
            );
}
