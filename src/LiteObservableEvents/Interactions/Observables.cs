using System.Reactive;

namespace LiteObservableEvents.Interactions;

/// <summary>
/// Provides commonly required, statically-allocated, pre-canned observables.
/// </summary>
internal static class Observables
{
    /// <summary>
    /// An observable that ticks <c>Unit.Default</c> as a single value.</summary>
    /// <remarks>
    /// <para>
    /// This observable is equivalent to <c>Observable&lt;Unit&gt;.Default</c>, but is provided for convenience.
    /// </para>
    /// </remarks>
    public static readonly IObservable<Unit> Unit = Observable<Unit>.Default;
}
