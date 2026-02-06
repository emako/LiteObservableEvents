using System.Reactive.Disposables;

namespace LiteObservableEvents;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

public class ObservableEventHub : IDisposable
{
    public static ObservableEventHub Default { get; } = new();

    protected readonly CompositeDisposable _subscriptions = [];

    /// <summary>
    /// Disposes all managed subscriptions.
    /// </summary>
    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public IDisposable Subscribe(IDisposable subscription)
    {
        _subscriptions.Add(subscription);
        return subscription;
    }
}

#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore IDE0079 // Remove unnecessary suppression
