[![NuGet](https://img.shields.io/nuget/v/LiteObservableEvents.svg)](https://nuget.org/packages/LiteObservableEvents) [![Actions](https://github.com/emako/LiteObservableEvents/actions/workflows/library.nuget.yml/badge.svg)](https://github.com/emako/LiteObservableEvents/actions/workflows/library.nuget.yml)

# LiteObservableEvents

Lightweight observable event hub and source-generated event bridges for .NET, with optional weak-reference (holder) lifecycle and MVVM-style interactions.

## Features

- **Event hubs**: `StrongReferenceEventHub` and `WeakReferenceEventHub` for central subscription management.
- **Holder-based lifecycle**: Subscribe with a holder object; when the holder is GC'd, subscriptions are cleaned up automatically (WeakReferenceMessenger-style).
- **Source-generated `Events()`**: Generate type-safe, observable wrappers for instance/static events via source generator.
- **Interactions**: `LiteObservableEvents.Interactions` for ViewModel–View communication (e.g. confirmation dialogs).

## Demos (WpfApp)

The **WpfApp** project is a small WPF demo that shows the following.

### 1. WeakReferenceEventHub + holder

Subscriptions use `holder: this` (the window). On window close, all subscriptions for that holder are disposed. `Cleanup()` or new subscriptions remove dead holder subscriptions.

```c#
this.Events().Closed
    .Subscribe(_ =>
    {
        _confirmInteractionRegistration?.Dispose();
        WeakReferenceEventHub.Default.UnsubscribeAll(this);
        ViewModel?.Dispose();
    });
```

### 2. Static event subscription

**Hand-written add/remove:**

```c#
WeakReferenceEventHub.Default.Subscribe<bool, bool>(
    holder: this,
    addHandler: h => MyStaticObject.MyStaticEvent += h,
    removeHandler: h => MyStaticObject.MyStaticEvent -= h,
    onNext: _ => Debug.WriteLine("Raised - MyStaticObject.MyStaticEvent += h"));
```

**Reflection:**

```c#
WeakReferenceEventHub.Default.Subscribe<bool>(
    holder: this,
    target: typeof(MyStaticObject),
    eventName: nameof(MyStaticObject.MyStaticEvent),
    onNext: _ => Debug.WriteLine("Raised - MyStaticObject.MyStaticEvent reflection"));
```

### 3. Static object's event (singleton)

**Source-generated `Events()`:**

```c#
WeakReferenceEventHub.Default.Subscribe(this, MySingletonObject.Instance.Events().MyEvent, _ =>
{
    Debug.WriteLine("Raised - MySingletonObject.Instance.Events().MyEvent source-generated");
});
```

**Hand-written add/remove:**

```c#
WeakReferenceEventHub.Default.Subscribe<bool, bool>(
    holder: this,
    addHandler: h => MySingletonObject.Instance.MyEvent += h,
    removeHandler: h => MySingletonObject.Instance.MyEvent -= h,
    onNext: _ => Debug.WriteLine("Raised - MySingletonObject.Instance.MyEvent += h"));
```

**Reflection:**

```c#
WeakReferenceEventHub.Default.Subscribe<bool>(
    holder: this,
    target: MySingletonObject.Instance,
    eventName: nameof(MySingletonObject.MyEvent),
    onNext: _ => Debug.WriteLine("Raised - MySingletonObject.Instance.MyEvent reflection"));
```

### 4. Interactions (ViewModel ↔ View)

ViewModel exposes an `Interaction<string, bool>`. View registers a handler (e.g. MessageBox); a command calls `Handle(...).ToTask()` and shows the result.

**View — register handler:**

```c#
_confirmInteractionRegistration = ViewModel.ConfirmAction.RegisterHandler(context =>
{
    var result = MessageBox.Show(context.Input, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
    context.SetOutput(result == MessageBoxResult.Yes);
});
```

**ViewModel — declare and invoke:**

```c#
public Interaction<string, bool> ConfirmAction { get; } = new();

[RelayCommand]
private async Task AskConfirmAsync()
{
    var approved = await ConfirmAction.Handle("Do you want to continue?").ToTask();
    LastResult = approved ? "User chose: Yes" : "User chose: No";
}
```

### 5. Raising events from ViewModel

A command toggles a bool and raises both static and singleton events; the UI binds to the toggle value.

```c#
[ObservableProperty]
private bool _staticEventToggle;

[RelayCommand]
private void RaiseStaticEvents()
{
    StaticEventToggle = !StaticEventToggle;
    MyStaticObject.RaiseSomeEvent(StaticEventToggle);
    MySingletonObject.Instance.RaiseSomeEvent(StaticEventToggle);
}
```

Run **WpfApp** and use the two buttons to try the confirmation interaction and the static/static-object event raise; check Debug output for subscription messages.

## Attribution

Portions of this project are derived from or inspired by:

- [ReactiveUI](https://github.com/reactiveui/ReactiveUI) (e.g. Interaction pattern)
- [reactivemarbles/ObservableEvents](https://github.com/reactivemarbles/ObservableEvents)

See [LICENSE](LICENSE) for details.

## License

[MIT](LICENSE)
