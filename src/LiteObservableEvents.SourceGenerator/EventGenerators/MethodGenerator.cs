using static LiteObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace LiteObservableEvents.SourceGenerator.EventGenerators;

internal static class MethodGenerator
{
    public static MethodDeclarationSyntax GenerateMethod(this INamedTypeSymbol declarationType)
    {
        return declarationType.IsStatic ? GenerateStaticMethod(declarationType) : GenerateInstanceMethod(declarationType);
    }

    private static MethodDeclarationSyntax GenerateStaticMethod(INamedTypeSymbol declarationType)
    {
        var eventsClassName = "global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events";
        var modifiers = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword };
        var parameters = new[] { Parameter(declarationType.GenerateFullGenericName(), "item", [SyntaxKind.ThisKeyword]) };
        var body = ArrowExpressionClause(ObjectCreationExpression(eventsClassName, [Argument("item")]));
        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(declarationType);

        return MethodDeclaration(attributes, modifiers, eventsClassName, "Events", parameters, 0, body)
            .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
    }

    private static MethodDeclarationSyntax GenerateInstanceMethod(INamedTypeSymbol declarationType)
    {
        var eventsClassName = "global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events";
        var modifiers = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword };
        var parameters = new[] { Parameter(declarationType.GetTypeSyntax(), "item", [SyntaxKind.ThisKeyword]) };
        var typeParameterConstraints = declarationType.GetTypeParameterConstraints();
        var typeParameters = declarationType.GetTypeParametersAsTypeParameterSyntax();
        var returnTypeSyntax = declarationType.GetTypeSyntax(eventsClassName);
        var body = ArrowExpressionClause(ObjectCreationExpression(returnTypeSyntax, [Argument("item")]));
        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(declarationType);

        return MethodDeclaration(attributes, modifiers, returnTypeSyntax, "Events", parameters, typeParameterConstraints, typeParameters, 0, body)
            .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
    }
}
