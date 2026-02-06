namespace LiteObservableEvents.SourceGenerator;

internal static class DiagnosticWarnings
{
    internal static readonly DiagnosticDescriptor EventsNotFound = new(
        id: "LITEOBS001",
        title: "Events not found",
        messageFormat: "Events not be found on the target type",
        category: "Compiler",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
