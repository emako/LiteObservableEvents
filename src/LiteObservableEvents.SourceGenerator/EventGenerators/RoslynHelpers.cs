using static LiteObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace LiteObservableEvents.SourceGenerator.EventGenerators;

/// <summary>
/// Helper methods associated with the roslyn template generators.
/// </summary>
internal static class RoslynHelpers
{
    internal const string ObservableUnitName = "global::System.Reactive.Unit";
    internal const string VoidType = "System.Void";

    public static SymbolDisplayFormat SymbolDisplayFormat { get; } = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance);

    public static SymbolDisplayFormat GenericTypeFormat { get; } = new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static SymbolDisplayFormat TypeFormat { get; } = new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.None, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    /// <summary>
    /// Gets an argument which access System.Reactive.Unit.Default member.
    /// </summary>
    public static ArgumentSyntax[] ReactiveUnitArgumentList { get; } = [Argument(ObservableUnitName + ".Default")];

    public static Func<INamedTypeSymbol, bool> HasEvents { get; } = symbol => symbol.GetMembers().OfType<IEventSymbol>().Any(x => x.DeclaredAccessibility == Accessibility.Public);

    /// <summary>
    /// Gets information about the event's obsolete information if any.
    /// </summary>
    /// <param name="eventDetails">The event details.</param>
    /// <returns>The event's obsolete information if there is any.</returns>
    public static AttributeListSyntax[] GenerateObsoleteAttributeList(ISymbol eventDetails)
    {
        var obsoleteAttribute = eventDetails.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.GetArityDisplayName().Equals("System.ObsoleteAttribute", StringComparison.InvariantCulture) ?? false);

        if (obsoleteAttribute == null)
        {
            return [];
        }

        var message = obsoleteAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
        var isError = bool.Parse(obsoleteAttribute.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString() ?? bool.FalseString) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        if (message != null && !string.IsNullOrWhiteSpace(message))
        {
            var attribute = Attribute(
                "global::System.ObsoleteAttribute",
                [AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(message))), AttributeArgument(LiteralExpression(isError))]);
            return [AttributeList(attribute)];
        }

        return [];
    }

    public static IReadOnlyCollection<TypeParameterSyntax> GetTypeParametersAsTypeParameterSyntax(
        this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetTypeParametersAs(TypeParameter);
    }

    public static IReadOnlyCollection<TypeSyntax> GetTypeParametersAsTypeSyntax(
        this INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetTypeParametersAs(name => SyntaxFactory.ParseTypeName(name));
    }

    public static TypeSyntax GetTypeSyntax(
        this ITypeSymbol typeSymbol,
        string? newName = null,
        IReadOnlyCollection<TypeSyntax>? genericTypeParameters = null)
    {
        string typeName = newName ?? typeSymbol.ToDisplayString(TypeFormat);

        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeParameters.Any())
        {
            return GenericName(typeName, genericTypeParameters ?? namedTypeSymbol.GetTypeParametersAsTypeSyntax());
        }

        return IdentifierName(typeName);
    }

    public static IReadOnlyCollection<TypeParameterConstraintClauseSyntax> GetTypeParameterConstraints(
        this INamedTypeSymbol typeSymbol)
    {
        return [.. typeSymbol.TypeParameters
            .Select(tp => (
                tp.Name,
                Constraints: tp.GetOtherConstrainst()
                    .Concat(tp.ConstraintTypes.Select(GetAsTypeConstraint))
                    .Select(tpc => tpc.WithTrailingTrivia(SyntaxFactory.Space).WithoutLeadingTrivia())
                    .ToArray()))
            .Where(tp => tp.Constraints.Length > 0)
            .Select(tp => TypeParameterConstraintClause(tp.Name, tp.Constraints))];
    }

    private static IEnumerable<TypeParameterConstraintSyntax> GetOtherConstrainst(
        this ITypeParameterSymbol typeSymbol)
    {
        if (typeSymbol.HasReferenceTypeConstraint ||
            typeSymbol.HasValueTypeConstraint)
        {
            yield return SyntaxFactory.ClassOrStructConstraint(
                typeSymbol.HasReferenceTypeConstraint ? SyntaxKind.ClassConstraint :
                typeSymbol.HasValueTypeConstraint ? SyntaxKind.StructConstraint :
                throw new InvalidOperationException());
        }

        if (typeSymbol.HasConstructorConstraint)
        {
            yield return SyntaxFactory.ConstructorConstraint();
        }
    }

    private static TypeParameterConstraintSyntax GetAsTypeConstraint(
        this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeArguments.Any())
        {
            return TypeConstraint(namedTypeSymbol.GetTypeSyntax(
                genericTypeParameters: [.. namedTypeSymbol.TypeArguments
                    .Select(tp => tp.GenerateFullGenericName())
                    .Select(IdentifierName)]));
        }

        return TypeConstraint(typeSymbol.GetTypeSyntax());
    }

    private static IReadOnlyCollection<T> GetTypeParametersAs<T>(
        this INamedTypeSymbol typeSymbol,
        Func<string, T> parseName)
    {
        return [.. typeSymbol.TypeParameters
            .Select(tp => tp.Name)
            .Select(parseName)];
    }
}
