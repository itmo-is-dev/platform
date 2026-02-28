using Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Testing.Behavioural.Analyzers.Syntax;

public sealed class GeneratedTestConstructorSyntax
{
    private readonly INamedTypeSymbol _contextSymbol;
    private readonly SourceTestFieldSyntax _sourceTestField;
    private readonly string _generatedTestName;

    private readonly ParameterSyntax _contextParameter;
    private readonly ParameterSyntax _outputParameter;
    private readonly ParameterSyntax[] _testConstructorParameters;
    private readonly ArgumentSyntax[] _testConstructorArguments;
    private readonly ObjectCreationExpressionSyntax _runnerCreation;

    public GeneratedTestConstructorSyntax(
        INamedTypeSymbol sourceTestSymbol,
        INamedTypeSymbol contextSymbol,
        SourceTestFieldSyntax sourceTestField,
        string generatedTestName)
    {
        _contextSymbol = contextSymbol;
        _sourceTestField = sourceTestField;
        _generatedTestName = generatedTestName;

        _contextParameter = Parameter(Identifier("context"))
            .WithType(IdentifierName(contextSymbol.GetTypeName()));

        _outputParameter = Parameter(Identifier("output"))
            .WithType(IdentifierName("Xunit.Abstractions.ITestOutputHelper"));

        ImmutableArray<IParameterSymbol> testConstructorParameters = sourceTestSymbol.Constructors is []
            ? []
            : sourceTestSymbol.Constructors[0].Parameters;

        _testConstructorParameters = testConstructorParameters
            .Select((parameter, i) => Parameter(Identifier($"arg{i}")).WithType(parameter.Type.GetTypeIdentifierName()))
            .ToArray();

        _testConstructorArguments = testConstructorParameters
            .Select((_, i) => Argument(IdentifierName($"arg{i}")))
            .ToArray();

        GenericNameSyntax runnerTypeName = GenericName(Constants.ScenarioRunner.FullName)
            .AddTypeArgumentListArguments(IdentifierName(_contextSymbol.GetTypeName()));

        _runnerCreation = ObjectCreationExpression(runnerTypeName)
            .AddArgumentListArguments(
                Argument(IdentifierName(_contextParameter.Identifier)),
                Argument(IdentifierName(_outputParameter.Identifier)));
    }

    public ConstructorDeclarationSyntax Declaration => ConstructorDeclaration(_generatedTestName)
        .AddModifiers(Token(SyntaxKind.PublicKeyword))
        .AddParameterListParameters(_contextParameter, _outputParameter)
        .AddParameterListParameters(_testConstructorParameters)
        .AddBodyStatements(
            ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(_sourceTestField.Name),
                _sourceTestField.Create(_testConstructorArguments))
            ),
            ExpressionStatement(InvocationExpression(
                RunnerAccessorSyntax.MethodAccessExpression(IdentifierName(_contextSymbol.GetTypeName())),
                ArgumentList()
                    .AddArguments(
                        Argument(IdentifierName(_sourceTestField.Name)),
                        Argument(_runnerCreation)))
            )
        );
}
