namespace LiteObservableEvents.SourceGenerator.EventGenerators.Comparers;

/// <summary>
/// A comparer which will compare names.
/// </summary>
internal class TypeDefinitionNameComparer : IEqualityComparer<INamedTypeSymbol>, IComparer<INamedTypeSymbol>
{
    public static TypeDefinitionNameComparer Default { get; } = new TypeDefinitionNameComparer();

    /// <inheritdoc/>
    public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y)
    {
        return StringComparer.Ordinal.Equals(x?.GenerateFullGenericName(), y?.GenerateFullGenericName());
    }

    /// <inheritdoc/>
    public int GetHashCode(INamedTypeSymbol obj)
    {
        return StringComparer.Ordinal.GetHashCode(obj.GenerateFullGenericName());
    }

    /// <inheritdoc/>
    public int Compare(INamedTypeSymbol? x, INamedTypeSymbol? y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        return string.CompareOrdinal(x.GenerateFullGenericName(), y.GenerateFullGenericName());
    }
}
