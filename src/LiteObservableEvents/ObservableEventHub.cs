using System.Reactive.Disposables;

namespace LiteObservableEvents;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

/// <summary>
/// Provides a hub for managing and subscribing to multiple observable events.
/// </summary>
public class ObservableEventHub : IDisposable
{
    /// <summary>
    /// Gets the default singleton instance of <see cref="ObservableEventHub"/>.
    /// </summary>
    public static ObservableEventHub Default { get; } = new();

    /// <summary>
    /// Stores all managed subscriptions for disposal.
    /// </summary>
    protected readonly CompositeDisposable _subscriptions = [];

    /// <summary>
    /// Disposes all managed subscriptions.
    /// </summary>
    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    /// <summary>
    /// Adds an existing subscription to the hub for management.
    /// </summary>
    /// <param name="subscription">The subscription to add.</param>
    /// <returns>The same <see cref="IDisposable"/> instance that was added.</returns>
    public IDisposable Subscribe(IDisposable subscription)
    {
        // Not necessarily ObservableEvent<> type, but allowed to join.
        _subscriptions.Add(subscription);
        return subscription;
    }

    /// <summary>
    /// Subscribes an observer to the specified observable and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="observable">The observable to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(IObservable<TEventArgs> observable, IObserver<TEventArgs> observer)
    {
        ObservableEvent<TEventArgs> observableEvent = new(observable);
        _subscriptions.Add(observableEvent.Subscribe(observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to the specified observable and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="observable">The observable to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(IObservable<TEventArgs> observable, Action<TEventArgs> onNext)
    {
        ObservableEvent<TEventArgs> observableEvent = new(observable);
        _subscriptions.Add(observableEvent.Subscribe(onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event on the specified target object and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object target, string eventName, IObserver<TEventArgs> observer)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(target, eventName, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event on the specified target object and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object target, string eventName, Action<TEventArgs> onNext)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(target, eventName, onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event using delegate add/remove handlers and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event using delegate add/remove handlers and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext)
    {
        ObservableEvent<TEventArgs> observableEvent = new();
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, onNext));
        return observableEvent;
    }
}

#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore IDE0079 // Remove unnecessary suppression
