namespace LiteObservableEvents.SourceGenerator.EventGenerators.Comparers;

internal class EventNameComparer : IComparer<IEventSymbol>, IEqualityComparer<IEventSymbol>
{
    public static EventNameComparer Default { get; } = new EventNameComparer();

    public int Compare(IEventSymbol x, IEventSymbol y)
    {
        return string.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
    }

    public bool Equals(IEventSymbol x, IEventSymbol y)
    {
        return string.Equals(x.Name, y.Name, StringComparison.InvariantCulture);
    }

    public int GetHashCode(IEventSymbol obj)
    {
        return obj.Name.GetHashCode();
    }
}
