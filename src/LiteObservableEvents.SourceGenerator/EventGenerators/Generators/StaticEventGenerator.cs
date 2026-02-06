using static LiteObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace LiteObservableEvents.SourceGenerator.EventGenerators.Generators;

internal class StaticEventGenerator : EventGeneratorBase
{
    /// <inheritdoc/>
    public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, Func<string?, INamedTypeSymbol?> getSymbolOf)
    {
        var eventWrapperMembers = new List<PropertyDeclarationSyntax>();

        var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

        foreach (var eventDetail in item.GetEvents(getSymbolOf, true))
        {
            var eventWrapper = GenerateEventWrapperObservable(eventDetail, item.GenerateFullGenericName(), item.Name);

            if (eventWrapper != null)
            {
                eventWrapperMembers.Add(eventWrapper);
            }
        }

        if (eventWrapperMembers.Count > 0)
        {
            var members = new[]
            {
                ClassDeclaration(
                    "RxEvents",
                    [SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword],
                    [.. eventWrapperMembers.Where(x => x != null).Select(x => (MemberDeclarationSyntax)x!)],
                    1)
            };

            return NamespaceDeclaration(namespaceName, members, true);
        }

        return null;
    }
}
