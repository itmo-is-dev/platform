using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SourceKit;
using SourceKit.Extensions;
using SourceKit.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Options.Analyzers;

[Generator]
public sealed class OptionRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<string, string> optionRegistrations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                static (context, _) =>
                {
                    var syntax = (InvocationExpressionSyntax)context.Node;

                    if (syntax.Expression is not MemberAccessExpressionSyntax memberAccess)
                        return IncrementalResult.Skip;

                    if (memberAccess.Name.Identifier.Text is not "BindConfiguration")
                        return IncrementalResult.Skip;

                    var operation = context.SemanticModel.GetOperation(syntax);

                    if (operation is not IInvocationOperation invocationOperation)
                        return IncrementalResult.Skip;

                    if (invocationOperation.Arguments is not [var builderArgument, var argumentOperation, _])
                        return IncrementalResult.SkipWithMetadata(Log("invalid args"));

                    if (argumentOperation.Value.ConstantValue is not { HasValue: true, Value: string section })
                        return IncrementalResult.SkipWithMetadata(Log($"Invalid value {argumentOperation.Value}"));

                    if (builderArgument.Value.Type is null)
                        return IncrementalResult.SkipWithMetadata(Log("Invalid instance"));

                    var optionsBuilderType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                        "Microsoft.Extensions.Options.OptionsBuilder`1");

                    if (optionsBuilderType is null)
                        return IncrementalResult.SkipWithMetadata(Log("Builder type not found"));

                    var boundOptionsBuilderType = builderArgument.Value.Type.FindAssignableTypeConstructedFrom(
                        optionsBuilderType);

                    if (boundOptionsBuilderType is null)
                        return IncrementalResult.Skip;

                    var optionsType = boundOptionsBuilderType.TypeArguments.Single();

                    return IncrementalResult.Success((section,
                        optionsType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))));
                })
            .Unwrap(context);

        context.RegisterSourceOutput(
            optionRegistrations.Collect(),
            static (context, optionRegistrations) =>
            {
                if (optionRegistrations is [])
                    return;

                var attributes = optionRegistrations
                    .Select(registration =>
                    {
                        var sectionArgument = AttributeArgument(LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(registration.Item1)));

                        var typeArgument = AttributeArgument(LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(registration.Item2)));

                        return AttributeList()
                            .AddAttributes(
                                Attribute(IdentifierName("global::Itmo.Dev.Platform.Options.OptionRegistration"))
                                    .AddArgumentListArguments(sectionArgument, typeArgument))
                            .WithTarget(AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)));
                    })
                    .ToArray();

                var unit = CompilationUnit()
                    .AddAttributeLists(attributes);

                context.AddSource(
                    "OptionRegistrations.g.cs",
                    unit.NormalizeWhitespace(eol: "\n").ToFullString());
            });
    }

    private static Diagnostic Log(string message) => LoggingDiagnostic.Create(message);
}
