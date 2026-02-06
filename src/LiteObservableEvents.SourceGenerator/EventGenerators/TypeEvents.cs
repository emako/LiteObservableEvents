using LiteObservableEvents.SourceGenerator.EventGenerators.Comparers;

namespace LiteObservableEvents.SourceGenerator.EventGenerators;

internal readonly struct TypeEvents(INamedTypeSymbol type, IEnumerable<IEventSymbol> events) : IComparable<TypeEvents>
{
    public INamedTypeSymbol Type { get; } = type;

    public IReadOnlyList<IEventSymbol> Events { get; } = [.. events];

    public int CompareTo(TypeEvents other)
    {
        return TypeEventsComparer.Default.Compare(other, this);
    }
}
