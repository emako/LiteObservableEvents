using System.Reactive.Linq;

namespace LiteObservableEvents.Interactions;

/// <summary>
/// Provides commonly required, statically-allocated, pre-canned observables.
/// </summary>
/// <typeparam name="T">
/// The observable type.
/// </typeparam>
internal static class Observable<T>
{
    /// <summary>
    /// An observable of type <typeparamref name="T"/> that ticks a single, default value.
    /// </summary>
    public static readonly IObservable<T> Default = Observable.Return(default(T)!);
}
