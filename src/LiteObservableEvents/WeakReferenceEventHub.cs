using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;

namespace LiteObservableEvents;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
public class WeakReferenceEventHub
{
    /// <summary>
    /// Gets the default singleton instance of <see cref="WeakReferenceEventHub"/>.
    /// </summary>
    public static WeakReferenceEventHub Default { get; } = new();

    /// <summary>
    /// Stores all managed subscriptions for disposal.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    protected readonly CompositeDisposable _subscriptions = [];

    /// <summary>
    /// Removes and disposes all subscriptions whose holder has been garbage-collected.
    /// Call this explicitly to reclaim resources, or rely on automatic cleanup when subscribing with a holder.
    /// </summary>
    public void Cleanup()
    {
        CleanupDeadHolders();
    }

    /// <summary>
    /// Disposes and removes from the hub any subscription whose holder is no longer alive (has been GC'd).
    /// </summary>
    protected void CleanupDeadHolders()
    {
        _subscriptions.Where(subscription =>
            subscription is IObservableEvent observableEvent
                && observableEvent.Holder is { } weakReference
                && !weakReference.TryGetTarget(out _))
            .ToList()
            .ForEach(subscription =>
            {
                subscription.Dispose();
                _subscriptions.Remove(subscription);
            });
    }

    /// <summary>
    /// Subscribes an observer to the specified observable and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="observable">The observable to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object? holder, IObservable<TEventArgs> observable, IObserver<TEventArgs> observer)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder, observable);
        _subscriptions.Add(observableEvent.Subscribe(observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to the specified observable and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="observable">The observable to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object? holder, IObservable<TEventArgs> observable, Action<TEventArgs> onNext)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder, observable);
        _subscriptions.Add(observableEvent.Subscribe(onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event on the specified target object and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object? holder, object target, string eventName, IObserver<TEventArgs> observer)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(target, eventName, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event on the specified target object and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs>(object? holder, object target, string eventName, Action<TEventArgs> onNext)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(target, eventName, onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event using delegate add/remove handlers and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(object? holder, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event using delegate add/remove handlers and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(object? holder, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, onNext));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an observer to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(object? holder, Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, observer));
        return observableEvent;
    }

    /// <summary>
    /// Subscribes an action to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and manages the subscription.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
    public IDisposable Subscribe<TEventArgs, TDelegate>(object? holder, Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext)
    {
        CleanupDeadHolders();
        ObservableEvent<TEventArgs> observableEvent = new(holder);
        _subscriptions.Add(observableEvent.Subscribe(addHandler, removeHandler, onNext));
        return observableEvent;
    }

    /// <summary>
    /// Unsubscribes and disposes all holder-managed subscriptions without disposing the <see cref="_subscriptions"/> collection itself.
    /// This allows the hub to continue managing new subscriptions after clearing existing ones.
    /// <param name="holder">The holder object associated with this subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// </summary>
    public void UnsubscribeAll(object? holder)
    {
        if (holder is null) return;

        _subscriptions
            .Where(subscription => subscription is IObservableEvent observableEvent
                && observableEvent.Holder is { } weakReference
                && weakReference.TryGetTarget(out object? target)
                && ReferenceEquals(target, holder))
            .ToList()
            .ForEach(subscription =>
            {
                subscription.Dispose();
                _subscriptions.Remove(subscription);
            });
    }
}

public static class ObservableEventHubExtensions
{
    /// <summary>
    /// Attaches an optional holder object to a subscription.
    /// </summary>
    /// <param name="subscription">
    /// The subscription returned from an observable sequence.
    /// </param>
    /// <param name="holder">
    /// An arbitrary object representing the logical owner of this subscription.
    /// This is typically used for debugging, diagnostics, or lifecycle tracking.
    /// </param>
    /// <returns>
    /// The original <see cref="IDisposable"/> subscription instance.
    /// </returns>
    /// <remarks>
    /// If the subscription implements <see cref="IObservableEvent"/>,
    /// the holder will be stored on the subscription instance.
    /// Otherwise, this method has no effect.
    ///
    /// This method does not affect the disposal semantics of the subscription.
    /// </remarks>
    public static IDisposable SetObservableEventHolder(this IDisposable subscription, object? holder)
    {
        if (subscription is IObservableEvent observableEvent)
        {
            observableEvent.Holder = new(holder);
        }
        return subscription;
    }
}
