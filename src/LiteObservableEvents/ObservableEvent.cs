using System.Reactive.Linq;
using System.Reflection;

namespace LiteObservableEvents;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
#pragma warning disable CA2208 // Instantiate argument exceptions correctly

public class ObservableEvent<TEventArgs> : IObservableEvent, IObservableEvent<TEventArgs>
{
    protected WeakReference<object?>? _holder = null;
    protected IObservable<TEventArgs>? _observable = null;
    protected IDisposable? _subscription = null;

    /// <inheritdoc/>
    public WeakReference<object?>? Holder
    {
        get => _holder;
        set => _holder = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{TEventArgs}"/> class.
    /// </summary>
    public ObservableEvent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{TEventArgs}"/> class with the specified holder object.
    /// </summary>
    /// <param name="holder">The holder object associated with this event subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    public ObservableEvent(object? holder)
    {
        _holder = new(holder);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{TEventArgs}"/> class with the specified observable.
    /// </summary>
    /// <param name="observable">The observable to subscribe to.</param>
    public ObservableEvent(IObservable<TEventArgs> observable)
    {
        _observable = observable;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableEvent{TEventArgs}"/> class with the specified holder object and observable.
    /// </summary>
    /// <param name="holder">The holder object associated with this event subscription. Used to track the owner of the subscription for group management or targeted unsubscription.</param>
    /// <param name="observable">The observable to subscribe to.</param>
    public ObservableEvent(object? holder, IObservable<TEventArgs> observable)
    {
        _holder = new(holder);
        _observable = observable;
    }

    /// <summary>
    /// Disposes the observable event and unsubscribes from the underlying observable.
    /// </summary>
    public virtual void Dispose()
    {
        _holder = null;
        _observable = null;
        _subscription?.Dispose();
        _subscription = null;
    }

    /// <summary>
    /// Subscribes the specified observer to the current observable.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(IObserver<TEventArgs> observer)
    {
        if (_observable is null)
            throw new ArgumentNullException(nameof(_observable), "Observable is not set. Use a different Subscribe overload to set the observable.");

        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    /// <summary>
    /// Subscribes the specified action to the current observable.
    /// </summary>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(Action<TEventArgs> onNext)
    {
        if (_observable is null)
            throw new ArgumentNullException(nameof(_observable), "Observable is not set. Use a different Subscribe overload to set the observable.");

        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    /// <summary>
    /// Subscribes to an event on the specified target object using the event name and observer.
    /// </summary>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEventInfo(target, eventName);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    /// <summary>
    /// Subscribes to an event on the specified target object using the event name and action.
    /// </summary>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEventInfo(target, eventName);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    /// <summary>
    /// Subscribes to an event using delegate add/remove handlers and an observer.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEventPattern(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    /// <summary>
    /// Subscribes to an event using delegate add/remove handlers and an action.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEventPattern(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    /// <summary>
    /// Subscribes to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and an observer.
    /// </summary>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEvent(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    /// <summary>
    /// Subscribes to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and an action.
    /// </summary>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEvent(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    /// <summary>
    /// Creates an observable from an event on the specified target object using reflection.
    /// </summary>
    /// <param name="target">The target object containing the event or a type of static event class.</param>
    /// <param name="eventName">The name of the event to observe.</param>
    /// <returns>An observable sequence of event arguments.</returns>
    protected static IObservable<TEventArgs> CreateObservableFromEventInfo(object target, string eventName)
    {
        Delegate? del = null;

        // This handles both Action<T> and EventHandler<T> style events.
        // For EventHandler, T will be the EventArgs type.
        // For Action<T>, T will be the parameter type.
        IObservable<TEventArgs> observable = Observable.FromEvent<TEventArgs>(
            handler => // Add handler
            {
                if (target is Type type)
                {
                    EventInfo eventInfo = type.GetEvent(eventName)
                        ?? throw new ArgumentException($"Event '{eventName}' not found on target type '{target.GetType().Name}'.");

                    del = Delegate.CreateDelegate(eventInfo.EventHandlerType!, handler, nameof(handler.Invoke));
                    eventInfo.AddEventHandler(target, del);
                }
                else
                {
                    EventInfo eventInfo = target.GetType().GetEvent(eventName)
                        ?? throw new ArgumentException($"Event '{eventName}' not found on target type '{target.GetType().Name}'.");

                    del = Delegate.CreateDelegate(eventInfo.EventHandlerType!, handler, nameof(handler.Invoke));
                    eventInfo.AddEventHandler(target, del);
                }
            },
            handler => // Remove handler
            {
                if (target is Type type)
                {
                    EventInfo? eventInfo = type.GetEvent(eventName);

                    if (eventInfo is null) return; // Should not happen if add worked
                    if (del is not null) eventInfo.RemoveEventHandler(target, del);
                }
                else
                {
                    EventInfo? eventInfo = target.GetType().GetEvent(eventName);

                    if (eventInfo is null) return; // Should not happen if add worked
                    if (del is not null) eventInfo.RemoveEventHandler(target, del);
                }
            });

        return observable;
    }

    /// <summary>
    /// Creates an observable from an event pattern using delegate add/remove handlers.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <returns>An observable sequence of event arguments.</returns>
    protected static IObservable<TEventArgs> CreateObservableFromEventPattern<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    {
        IObservable<TEventArgs> observable = Observable.FromEventPattern<TDelegate, TEventArgs>(addHandler, removeHandler)
            .Select(e => e.EventArgs);
        return observable;
    }

    /// <summary>
    /// Creates an observable from add/remove handlers for <see cref="Action{TEventArgs}"/>.
    /// </summary>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <returns>An observable sequence of event arguments.</returns>
    protected static IObservable<TEventArgs> CreateObservableFromEvent(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler)
    {
        IObservable<TEventArgs> observable = Observable.FromEvent(addHandler, removeHandler);
        return observable;
    }
}

#pragma warning restore CA2208 // Instantiate argument exceptions correctly
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore IDE0079 // Remove unnecessary suppression

/// <summary>
/// Represents an observable event abstraction with an optional holder reference.
/// </summary>
public interface IObservableEvent : IDisposable
{
    /// <summary>
    /// Gets or sets the holder object that owns or is associated with this event subscription.
    /// This is typically used to track the owner of the subscription.
    /// </summary>
    public WeakReference<object?>? Holder { get; set; }
}

/// <summary>
/// Represents an observable event abstraction that allows subscribing to events in various ways.
/// </summary>
/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
public interface IObservableEvent<TEventArgs> : IDisposable
{
    /// <summary>
    /// Subscribes to an event on the specified target object using the event name and observer.
    /// </summary>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, IObserver<TEventArgs> observer);

    /// <summary>
    /// Subscribes to an event on the specified target object using the event name and action.
    /// </summary>
    /// <param name="target">The target object containing the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, Action<TEventArgs> onNext);

    /// <summary>
    /// Subscribes to an event using delegate add/remove handlers and an observer.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer);

    /// <summary>
    /// Subscribes to an event using delegate add/remove handlers and an action.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate type of the event handler.</typeparam>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext);

    /// <summary>
    /// Subscribes to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and an observer.
    /// </summary>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer);

    /// <summary>
    /// Subscribes to an event using add/remove handlers for <see cref="Action{TEventArgs}"/> and an action.
    /// </summary>
    /// <param name="addHandler">The action to add the event handler.</param>
    /// <param name="removeHandler">The action to remove the event handler.</param>
    /// <param name="onNext">The action to invoke for each event.</param>
    /// <returns>The current <see cref="ObservableEvent{TEventArgs}"/> instance.</returns>
    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext);
}
