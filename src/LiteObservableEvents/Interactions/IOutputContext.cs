namespace LiteObservableEvents.Interactions;

/// <summary>
/// Gives the ability to get the output.
/// </summary>
/// <typeparam name="TInput">The input.</typeparam>
/// <typeparam name="TOutput">The output.</typeparam>
public interface IOutputContext<out TInput, TOutput> : IInteractionContext<TInput, TOutput>
{
    /// <summary>
    /// Gets the output of the interaction.
    /// </summary>
    /// <returns>
    /// The output.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// If the output has not been set.
    /// </exception>
    TOutput GetOutput();
}
