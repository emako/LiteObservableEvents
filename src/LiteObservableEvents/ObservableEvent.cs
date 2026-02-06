using System.Reactive.Linq;
using System.Reflection;

namespace LiteObservableEvents;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

public class ObservableEvent<TEventArgs>(IObservable<TEventArgs> observable) : IObservableEvent<TEventArgs>
{
    protected IObservable<TEventArgs>? _observable = observable;
    protected IDisposable? _subscription = null;

    public virtual void Dispose()
    {
        _observable = null;
        _subscription?.Dispose();
        _subscription = null;
    }

    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEventInfo(target, eventName);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEventInfo(target, eventName);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEventPattern(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEventPattern(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer)
    {
        _observable = CreateObservableFromEvent(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(observer);
        return this;
    }

    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext)
    {
        _observable = CreateObservableFromEvent(addHandler, removeHandler);
        _subscription?.Dispose();
        _subscription = _observable?.Subscribe(onNext);
        return this;
    }

    protected static IObservable<TEventArgs> CreateObservableFromEventInfo(object target, string eventName)
    {
        // This handles both Action<T> and EventHandler<T> style events.
        // For EventHandler, T will be the EventArgs type.
        // For Action<T>, T will be the parameter type.
        IObservable<TEventArgs> observable = Observable.FromEvent<TEventArgs>(
            handler =>
            {
                EventInfo eventInfo = target.GetType().GetEvent(eventName)
                    ?? throw new ArgumentException($"Event '{eventName}' not found on target type '{target.GetType().Name}'.");
                Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType!, handler, nameof(handler.Invoke));
                eventInfo.AddEventHandler(target, del);
            },
            handler =>
            {
                EventInfo? eventInfo = target.GetType().GetEvent(eventName);
                if (eventInfo is null) return; // Should not happen if add worked

                Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType!, handler, nameof(handler.Invoke));
                eventInfo.RemoveEventHandler(target, del);
            });

        return observable;
    }

    protected static IObservable<TEventArgs> CreateObservableFromEventPattern<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
    {
        IObservable<TEventArgs> observable = Observable.FromEventPattern<TDelegate, TEventArgs>(addHandler, removeHandler)
            .Select(e => e.EventArgs);
        return observable;
    }

    protected static IObservable<TEventArgs> CreateObservableFromEvent(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler)
    {
        IObservable<TEventArgs> observable = Observable.FromEvent(addHandler, removeHandler);
        return observable;
    }
}

#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore IDE0079 // Remove unnecessary suppression

public interface IObservableEvent<TEventArgs> : IDisposable
{
    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, IObserver<TEventArgs> observer);

    public ObservableEvent<TEventArgs> Subscribe(object target, string eventName, Action<TEventArgs> onNext);

    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IObserver<TEventArgs> observer);

    public ObservableEvent<TEventArgs> Subscribe<TDelegate>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, Action<TEventArgs> onNext);

    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IObserver<TEventArgs> observer);

    public ObservableEvent<TEventArgs> Subscribe(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, Action<TEventArgs> onNext);
}
