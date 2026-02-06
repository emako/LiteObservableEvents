namespace LiteObservableEvents.SourceGenerator;

/// <summary>
/// Roslyn 4.0 generator for generating events.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class EventGenerator40 : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("LiteObservableEvents.SourceGenerated.cs", ClassConstants.ExtensionMethodText));

        var candidateInvocations =
            context.SyntaxProvider.CreateSyntaxProvider(
                (syntax, _) => syntax.IsCandidate(),
                (syntax, _) => (InvocationExpressionSyntax)syntax.Node);

        var inputs = candidateInvocations.Collect()
            .Combine(context.CompilationProvider)
            .Select((combined, _) => (Candidates: combined.Left, Compilation: combined.Right));

        context.RegisterSourceOutput(
            inputs,
            (generateContext, collectedValues) =>
                EventGenerator.Generate(collectedValues.Compilation, collectedValues.Candidates, generateContext.AddSource, generateContext.ReportDiagnostic, generateContext.CancellationToken));
    }
}
