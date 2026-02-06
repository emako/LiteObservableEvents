using LiteObservableEvents.SourceGenerator.EventGenerators.Comparers;
using static LiteObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace LiteObservableEvents.SourceGenerator.EventGenerators.Generators;

internal class InstanceEventGenerator : EventGeneratorBase
{
    private const string DataFieldName = "_data";

    /// <inheritdoc/>
    public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, Func<string?, INamedTypeSymbol?> getSymbolOf)
    {
        var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

        var eventWrapperes = GenerateEventWrapperClasses(item, [.. item.GetEvents(getSymbolOf)]).ToList();

        if (eventWrapperes.Count > 0)
        {
            return NamespaceDeclaration(namespaceName, eventWrapperes, true);
        }

        return null;
    }

    private static ConstructorDeclarationSyntax GenerateEventWrapperClassConstructor(INamedTypeSymbol typeDefinition)
    {
        const string dataParameterName = "data";
        string className = "Rx" + typeDefinition.Name + "Events";

        var constructorBlock = Block(
            [
                ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, DataFieldName, "data"))
            ],
            2);

        return ConstructorDeclaration(default, [SyntaxKind.PublicKeyword], [Parameter(typeDefinition.GetTypeSyntax(), dataParameterName)], className, constructorBlock, 1)
            .WithLeadingTrivia(
                XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Initializes a new instance of the {0} class.", className, (dataParameterName, "The class that is being wrapped.")));
    }

    private static FieldDeclarationSyntax GenerateEventWrapperField(INamedTypeSymbol typeDefinition)
    {
        return FieldDeclaration(
            typeDefinition.GetTypeSyntax(),
            DataFieldName,
            [SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword],
            1);
    }

    private static IEnumerable<ClassDeclarationSyntax> GenerateEventWrapperClasses(INamedTypeSymbol typeDefinition, IReadOnlyList<IEventSymbol> events)
    {
        var members = new List<MemberDeclarationSyntax> { GenerateEventWrapperField(typeDefinition), GenerateEventWrapperClassConstructor(typeDefinition) };

        if (events.Count == 0)
        {
            yield break;
        }

        var properties = new List<PropertyDeclarationSyntax>(events.Count);

        for (int i = 0; i < events.Count; ++i)
        {
            var eventSymbol = events[i];

            var eventWrapper = GenerateEventWrapperObservable(eventSymbol, DataFieldName, null);

            if (eventWrapper == null)
            {
                continue;
            }

            properties.Add(eventWrapper);
        }

        var obsoleteList = RoslynHelpers.GenerateObsoleteAttributeList(typeDefinition);

        if (properties.Count > 0)
        {
            yield return ClassDeclaration(
                "Rx" + typeDefinition.Name + "Events",
                obsoleteList,
                [SyntaxKind.InternalKeyword],
                [.. members, .. properties],
                typeDefinition.GetTypeParameterConstraints(),
                typeDefinition.GetTypeParametersAsTypeParameterSyntax(),
                1);
        }
    }

    private class TypeArguments(INamedTypeSymbol[] typeArguments) : IEquatable<TypeArguments>
    {
        public INamedTypeSymbol[] Types { get; } = typeArguments;

        public bool Equals(TypeArguments other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Types.Length != Types.Length)
            {
                return false;
            }

            for (int i = 0; i < Types.Length; ++i)
            {
                if (!TypeNameComparer.Default.Equals(Types[i], other.Types[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 0;

                foreach (var item in Types)
                {
                    result = (result * 397) ^ TypeNameComparer.Default.GetHashCode(item);
                }

                return result;
            }
        }
    }
}
