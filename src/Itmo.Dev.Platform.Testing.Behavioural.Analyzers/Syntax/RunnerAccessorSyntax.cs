using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public static class RunnerAccessorSyntax
{
    private const string ClassName = "Accessor";
    private const string TypeParameterName = "TContext";
    private const string MethodName = "SetRunner";

    private static readonly AttributeSyntax UnsafeAccessorAttribute =
        Attribute(IdentifierName("System.Runtime.CompilerServices.UnsafeAccessor"))
            .AddArgumentListArguments(
                AttributeArgument(IdentifierName("System.Runtime.CompilerServices.UnsafeAccessorKind.Method")),
                AttributeArgument(
                    NameEquals("Name"),
                    nameColon: null,
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("set_Runner"))));

    private static readonly TypeSyntax FeatureBaseType =
        GenericName(Constants.FeatureBase.FullName)
            .AddTypeArgumentListArguments(IdentifierName(TypeParameterName));

    private static readonly TypeSyntax ScenarioRunnerType =
        GenericName(Constants.IScenarioRunner.FullName)
            .AddTypeArgumentListArguments(IdentifierName(TypeParameterName));

    public static readonly ClassDeclarationSyntax Declaration = ClassDeclaration(ClassName)
        .AddModifiers(Token(SyntaxKind.PrivateKeyword))
        .AddTypeParameterListParameters(TypeParameter(TypeParameterName))
        .AddConstraintClauses(
            TypeParameterConstraintClause(IdentifierName(TypeParameterName))
                .AddConstraints(TypeConstraint(IdentifierName(Constants.ITestContext.MetadataFullName)))
        )
        .AddMembers(
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), MethodName)
                .AddAttributeLists(AttributeList().AddAttributes(UnsafeAccessorAttribute))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.ExternKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("value")).WithType(FeatureBaseType),
                    Parameter(Identifier("runner")).WithType(ScenarioRunnerType))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        );

    public static ExpressionSyntax MethodAccessExpression(TypeSyntax contextType)
    {
        var className = GenericName(ClassName).AddTypeArgumentListArguments(contextType);

        return MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            className,
            IdentifierName(MethodName));
    }
}
