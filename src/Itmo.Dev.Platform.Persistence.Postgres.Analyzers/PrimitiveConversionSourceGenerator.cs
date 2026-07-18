using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Itmo.Dev.Platform.Persistence.Postgres.Analyzers;

[Generator]
public sealed class PrimitiveConversionSourceGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor InvalidPrimitiveConversion = new(
        id: "IID1001",
        title: "Primitive conversion is invalid",
        messageFormat: "Invalid primitive conversion: {0}",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly IdentifierNameSyntax ExtensionsTypeName = IdentifierName(
        "Itmo.Dev.Platform.Persistence.Postgres.Conversions.__UnsafePersistencePostgresConverterExtensions");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            FilterNode,
            CreateConversionRegistrationMethod);

        context.RegisterImplementationSourceOutput(
            provider.Collect(),
            static (context, registrations) =>
            {
                foreach (ConversionRegistrationMethod registration in registrations)
                {
                    var cls = ClassDeclaration(registration.Method.ContainingType.Name)
                        .AddModifiers(
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.StaticKeyword),
                            Token(SyntaxKind.PartialKeyword));

                    var method = registration.Syntax.WithBody(Block()).WithSemicolonToken(default);

                    if (method.ParameterList.Parameters is not [{ Type: SimpleNameSyntax name } configuratorParameter]
                        || name.Identifier.Text.EndsWith("IPostgresPersistenceConfigurator") is false
                        || configuratorParameter.Modifiers.Any(token => token.IsKind(SyntaxKind.ThisKeyword)) is false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            InvalidPrimitiveConversion,
                            registration.Syntax.ParameterList.GetLocation(),
                            $"conversion registration method should be an extensions method over IPostgresPersistenceConfigurator with a single parameter"));

                        continue;
                    }

                    var configuratorParameterName = configuratorParameter.Identifier;

                    foreach (ConversionDescriptor conversion in registration.Conversions)
                    {
                        var constructor = conversion.ValueObject.Constructors.SingleOrDefault(ctor =>
                            ctor.DeclaredAccessibility is Accessibility.Public
                            && ctor.Parameters is [var parameter]
                            && parameter.Type.Equals(
                                conversion.Primitive,
                                SymbolEqualityComparer.Default));

                        if (constructor is null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                InvalidPrimitiveConversion,
                                conversion.SyntaxReference?.GetSyntax().GetLocation(),
                                $"public constructor with single parameter of type '{conversion.Primitive.Name}' must be present in '{conversion.ValueObject.Name}'"));

                            continue;
                        }

                        var propertyName = constructor.Parameters[0].Name;
                        propertyName = $"{char.ToUpper(propertyName[0])}{propertyName[1..]}";

                        var property = conversion.ValueObject
                            .GetMembers()
                            .OfType<IPropertySymbol>()
                            .SingleOrDefault(prop =>
                                prop.DeclaredAccessibility is Accessibility.Public
                                && prop.Name == propertyName
                                && prop.Type.Equals(conversion.Primitive, SymbolEqualityComparer.Default));

                        if (property is null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                InvalidPrimitiveConversion,
                                conversion.SyntaxReference?.GetSyntax().GetLocation(),
                                $"public property '{conversion.Primitive.Name} {propertyName}' must be present in '{conversion.ValueObject.Name}'"));

                            continue;
                        }

                        var valueObjectTypeName = IdentifierName(
                            conversion.ValueObject.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                        var primitiveTypeName = IdentifierName(
                            conversion.Primitive.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                        var converterMethodName = conversion.ValueObject.IsValueType
                            ? GenericName("AddStructConverter")
                            : GenericName("AddConverter");

                        GenericNameSyntax converterMethod = converterMethodName.AddTypeArgumentListArguments(
                            valueObjectTypeName,
                            primitiveTypeName);

                        var extensionsMethod = MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ExtensionsTypeName,
                            converterMethod);

                        var wrap = SimpleLambdaExpression(Parameter(Identifier("value")))
                            .WithExpressionBody(ImplicitObjectCreationExpression()
                                .AddArgumentListArguments(Argument(IdentifierName("value"))));

                        var unwrap = SimpleLambdaExpression(Parameter(Identifier("value")))
                            .WithExpressionBody(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("value"),
                                IdentifierName(propertyName)));

                        method = method.AddBodyStatements(ExpressionStatement(InvocationExpression(extensionsMethod)
                            .AddArgumentListArguments(
                                Argument(IdentifierName(configuratorParameterName)),
                                Argument(wrap),
                                Argument(unwrap))));
                    }

                    method = method.AddBodyStatements(
                        ReturnStatement(IdentifierName(configuratorParameterName)));

                    cls = cls.AddMembers(method);

                    var ns = NamespaceDeclaration(
                            IdentifierName(registration.Method.ContainingType.ContainingNamespace.ToDisplayString(
                                SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                                    SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining))))
                        .AddMembers(cls)
                        .AddUsings(
                            registration.Syntax.Ancestors()
                                .Last()
                                .DescendantNodesAndSelf(node => node is not TypeDeclarationSyntax)
                                .OfType<UsingDirectiveSyntax>()
                                .ToArray());

                    context.AddSource(
                        $"{registration.Method.ContainingType.Name}.Conversion.g.cs",
                        ns.NormalizeWhitespace(eol: "\n").ToFullString());
                }
            });
    }

    private bool FilterNode(SyntaxNode node, CancellationToken _)
    {
        if (node is not MethodDeclarationSyntax methodSyntax)
            return false;

        if (methodSyntax.Modifiers.Any(SyntaxKind.StaticKeyword) is false
            || methodSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) is false)
        {
            return false;
        }

        return methodSyntax.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(attr => attr.Name)
            .OfType<SimpleNameSyntax>()
            .Any(name => name.Identifier.Text.StartsWith("GeneratePrimitiveConverter"));
    }

    private ConversionRegistrationMethod CreateConversionRegistrationMethod(
        GeneratorSyntaxContext context,
        CancellationToken _)
    {
        var methodSymbol = (IMethodSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node)!;

        var conversions = methodSymbol
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.Name.StartsWith("GeneratePrimitiveConverter") is true)
            .Where(attr => attr.AttributeClass?.TypeArguments.Length is 2)
            .Select(attr =>
            {
                var args = attr.AttributeClass!.TypeArguments;

                return new ConversionDescriptor(
                    attr.ApplicationSyntaxReference,
                    (INamedTypeSymbol)args[0],
                    (INamedTypeSymbol)args[1]);
            })
            .ToArray();

        return new ConversionRegistrationMethod((MethodDeclarationSyntax)context.Node, methodSymbol, conversions);
    }

    private readonly record struct ConversionDescriptor(
        SyntaxReference? SyntaxReference,
        INamedTypeSymbol ValueObject,
        INamedTypeSymbol Primitive);

    private readonly record struct ConversionRegistrationMethod(
        MethodDeclarationSyntax Syntax,
        IMethodSymbol Method,
        IReadOnlyCollection<ConversionDescriptor> Conversions);
}
