using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SourceKit;
using SourceKit.Extensions;
using SourceKit.Models;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Options.Analyzers;

[Generator]
public sealed class OptionRegistrationGenerator : IIncrementalGenerator
{
    private const string AttributeName = "OptionRegistrationAttribute";
    private const string AttributeMetadataName = $"global::Itmo.Dev.Platform.Options.{AttributeName}";

    private static readonly DiagnosticDescriptor MissingOptionsTypeAttributeDescriptor = new(
        id: "IID1000",
        title: "Options type missing [OptionsType] attribute",
        messageFormat: "Options type missing [OptionsType] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            GetRegistrationsFromCurrentAssembly(context).Collect(),
            static (context, registrations) => AddRegistrations(context, registrations, "Current"));

        context.RegisterSourceOutput(
            GetRegistrationsFromReferencedAssemblies(context).Collect(),
            static (context, registrations) => AddRegistrations(context, registrations, "Transitive"));

        context.RegisterSourceOutput(
            GetRegistrationsFromMethodProducedOptions(context).Collect(),
            static (context, registrations) => AddRegistrations(context, registrations, "MethodProduced"));

        return;

        static void AddRegistrations(
            SourceProductionContext context,
            ImmutableArray<OptionRegistration> optionRegistrations,
            string suffix)
        {
            if (optionRegistrations is [])
                return;

            var attributes = optionRegistrations
                .Select(registration =>
                {
                    var sectionArgument = AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal(registration.Section)));

                    var typeArgument = AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal(registration.TypeName)));

                    return AttributeList()
                        .AddAttributes(Attribute(IdentifierName(AttributeMetadataName))
                            .AddArgumentListArguments(sectionArgument, typeArgument))
                        .WithTarget(AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)));
                })
                .ToArray();

            var unit = CompilationUnit()
                .AddAttributeLists(attributes);

            context.AddSource($"OptionRegistrations.{suffix}.g.cs",
                unit.NormalizeWhitespace(eol: "\n").ToFullString());
        }
    }

    private static IncrementalValuesProvider<OptionRegistration> GetRegistrationsFromCurrentAssembly(
        IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<OptionRegistration> optionRegistrations = context.SyntaxProvider
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
                        return IncrementalResult.Skip;

                    if (argumentOperation.Value.ConstantValue is not { HasValue: true, Value: string section })
                        return IncrementalResult.Skip;

                    if (builderArgument.Value.Type is null)
                        return IncrementalResult.Skip;

                    var optionsBuilderType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                        "Microsoft.Extensions.Options.OptionsBuilder`1");

                    if (optionsBuilderType is null)
                        return IncrementalResult.Skip;

                    var optionsTypeAttributeType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                        "Itmo.Dev.Platform.Options.OptionsTypeAttribute");

                    if (optionsTypeAttributeType is null)
                        return IncrementalResult.Skip;

                    var boundOptionsBuilderType = builderArgument.Value.Type.FindAssignableTypeConstructedFrom(
                        optionsBuilderType);

                    if (boundOptionsBuilderType is null)
                        return IncrementalResult.Skip;

                    var optionsType = boundOptionsBuilderType.TypeArguments.Single();
                    var hasAttribute = optionsType.GetAttributes().HasAttribute(optionsTypeAttributeType);

                    var registration = new OptionRegistration(
                        section,
                        optionsType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                            SymbolDisplayGlobalNamespaceStyle.Omitted)));

                    return hasAttribute
                        ? IncrementalResult.Success(registration)
                        : IncrementalResult.Success(registration,
                            Diagnostic.Create(MissingOptionsTypeAttributeDescriptor, memberAccess.Name.GetLocation()));
                })
            .Unwrap(context);
        return optionRegistrations;
    }

    private static IncrementalValuesProvider<OptionRegistration> GetRegistrationsFromReferencedAssemblies(
        IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider
            .SelectMany(static compilation =>
            {
                return compilation.References
                    .Select(compilation.GetAssemblyOrModuleSymbol)
                    .OfType<IAssemblySymbol>();
            })
            .SelectMany(assembly => assembly.GetAttributes())
            .Where(attribute => attribute.AttributeClass?.Name is AttributeName)
            .Select(attribute =>
            {
                return new OptionRegistration(
                    (string)attribute.ConstructorArguments[0].Value!,
                    (string)attribute.ConstructorArguments[1].Value!);
            });
    }

    private static IncrementalValuesProvider<OptionRegistration> GetRegistrationsFromMethodProducedOptions(
        IncrementalGeneratorInitializationContext context)
    {
        const string sectionNameParameter = "SectionName";
        const string sectionParameterNameParameter = "SectionParameterName";

        return context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                static (context, _) =>
                {
                    var attributeOpenType = context.SemanticModel.Compilation.GetTypeByMetadataName(
                        "Itmo.Dev.Platform.Options.ProducesOptionRegistrationAttribute`1");

                    if (attributeOpenType is null)
                        return IncrementalResult.Skip;

                    var node = (InvocationExpressionSyntax)context.Node;
                    var operation = context.SemanticModel.GetOperation(node);

                    if (operation is not IInvocationOperation invocationOperation)
                        return IncrementalResult.Skip;

                    var targetMethod = invocationOperation.TargetMethod;
                    var attributes = targetMethod.GetAttributes();

                    var registrationAttributes = attributes
                        .Where(attr => SymbolEqualityComparer.Default.Equals(
                            attr.AttributeClass?.ConstructedFrom,
                            attributeOpenType));

                    var relevantAttributes = registrationAttributes
                        .Where(attr => attr.NamedArguments.Any(kvp
                            => kvp.Key is sectionNameParameter or sectionParameterNameParameter))
                        .Select(attr => (attr, invocationOperation, context.SemanticModel))
                        .ToImmutableArray();

                    return IncrementalResult.Success(relevantAttributes);
                })
            .Unwrap(context)
            .SelectMany(static attributes => attributes)
            .WithComparer(static (attribute, invocationOperation, _) => (attribute, invocationOperation))
            .Select(static (attribute, invocationOperation, semanticModel) =>
            {
                var sectionName = string.Empty;

                var sectionNameParameterArgument = attribute.NamedArguments
                    .SingleOrDefault(kvp => kvp.Key is sectionParameterNameParameter);

                if (sectionNameParameterArgument.Key is not null)
                {
                    if (sectionNameParameterArgument.Value.Value is not string parameterName)
                        return IncrementalResult.SkipWithMetadata(Log("invalid parameter name"));

                    var argument = invocationOperation.Arguments
                        .SingleOrDefault(arg => arg.Parameter?.Name == parameterName);

                    if (argument is null)
                        return IncrementalResult.SkipWithMetadata(Log("parameter not found in invocation"));

                    if (argument.Value.ConstantValue.HasValue is false
                        || argument.Value.ConstantValue.Value is not string parameterValue)
                    {
                        return IncrementalResult.SkipWithMetadata(Log($"invalid parameter value = {argument.ConstantValue}"));
                    }

                    sectionName = parameterValue;
                }

                var sectionNameArgument = attribute.NamedArguments
                    .SingleOrDefault(kvp => kvp.Key is sectionNameParameter);

                if (sectionNameArgument.Key is not null)
                {
                    if (sectionNameArgument.Value.Value is not string sectionNamePrefix)
                        return IncrementalResult.SkipWithMetadata(Log("Invalid section name prefix"));

                    sectionName = string.IsNullOrEmpty(sectionName)
                        ? sectionNamePrefix
                        : $"{sectionNamePrefix}:{sectionName}";
                }

                var optionsTypeAttributeType = semanticModel.Compilation.GetTypeByMetadataName(
                    "Itmo.Dev.Platform.Options.OptionsTypeAttribute");

                if (optionsTypeAttributeType is null)
                    return IncrementalResult.Skip;

                var optionsType = attribute.AttributeClass!.TypeArguments.Single();
                var hasAttribute = optionsType.GetAttributes().HasAttribute(optionsTypeAttributeType);

                var registration = new OptionRegistration(
                    sectionName,
                    optionsType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                        SymbolDisplayGlobalNamespaceStyle.Omitted)));

                return hasAttribute
                    ? IncrementalResult.Success(registration)
                    : IncrementalResult.Success(registration,
                        Diagnostic.Create(MissingOptionsTypeAttributeDescriptor, invocationOperation.Syntax.GetLocation()));
            })
            .Unwrap(context);
    }

    private static Diagnostic Log(string message) => LoggingDiagnostic.Create(message);
}
