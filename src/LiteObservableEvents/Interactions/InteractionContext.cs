namespace LiteObservableEvents.Interactions;

/// <summary>
/// Contains contextual information for an interaction.
/// </summary>
/// <remarks>
/// <para>
/// Instances of this class are passed into interaction handlers. The <see cref="Input"/> property exposes
/// the input to the interaction, whilst the <see cref="SetOutput"/> method allows a handler to provide the
/// output.
/// </para>
/// <para>
/// Calling <see cref="SetOutput"/> more than once throws an <see cref="InvalidOperationException"/>, ensuring the
/// handler's reply remains deterministic even when multiple handlers run concurrently. Use <see cref="IsHandled"/>
/// to guard logic that should only execute once.
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// <![CDATA[
/// viewModel.ConfirmDelete.RegisterHandler(async ctx =>
/// {
///     var approved = await dialogService.ShowAsync(ctx.Input);
///     ctx.SetOutput(approved);
/// });
/// ]]>
/// </code>
/// </example>
/// <typeparam name="TInput">
/// The type of the interaction's input.
/// </typeparam>
/// <typeparam name="TOutput">
/// The type of the interaction's output.
/// </typeparam>
public sealed class InteractionContext<TInput, TOutput> : IOutputContext<TInput, TOutput>
{
    private TOutput _output = default!;
    private int _outputSet;

    internal InteractionContext(TInput input) => Input = input;

    /// <inheritdoc />
    public TInput Input { get; }

    /// <inheritdoc />
    public bool IsHandled => _outputSet == 1;

    /// <inheritdoc />
    public void SetOutput(TOutput output)
    {
        if (Interlocked.CompareExchange(ref _outputSet, 1, 0) != 0)
        {
            throw new InvalidOperationException("Output has already been set.");
        }

        _output = output;
    }

    /// <inheritdoc />
    public TOutput GetOutput()
    {
        if (_outputSet == 0)
        {
            throw new InvalidOperationException("Output has not been set.");
        }

        return _output;
    }
}
