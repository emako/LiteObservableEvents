using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteObservableEvents;
using LiteObservableEvents.Interactions;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows;

namespace WpfApp;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    private IDisposable? _confirmInteractionRegistration;

    public MainWindow()
    {
        ThemeMode = ThemeMode.Dark;
        DataContext = ViewModel = new();
        InitializeComponent();

        // -------------------

        this.Events().Closed
            .Subscribe(_ =>
            {
                _confirmInteractionRegistration?.Dispose();
                WeakReferenceEventHub.Default.UnsubscribeAll(this);
                ViewModel?.Dispose();
            });

        // -------------------

        // [1] ObservableEvents demo: static event (hand-written add/remove only)
        WeakReferenceEventHub.Default.Subscribe<bool, bool>(
            holder: this,
            addHandler: h => MyStaticObject.MyStaticEvent += h,
            removeHandler: h => MyStaticObject.MyStaticEvent -= h,
            onNext: _ =>
            {
                Debug.WriteLine("Raised - MyStaticObject.MyStaticEvent += h");
            }
        );

        // [2] ObservableEvents demo: static object's event (use reflection)
        WeakReferenceEventHub.Default.Subscribe<bool>(
            holder: this,
            target: typeof(MyStaticObject),
            eventName: nameof(MyStaticObject.MyStaticEvent),
            onNext: _ =>
            {
                Debug.WriteLine("Raised - MyStaticObject.MyStaticEvent reflection");
            }
        );

        // -------------------

        // [1] ObservableEvents demo: static object's event (source-generated Events())
        WeakReferenceEventHub.Default.Subscribe(this, MySingletonObject.Instance.Events().MyEvent, _ =>
        {
            Debug.WriteLine("Raised - MySingletonObject.Instance.Events().MyEvent source-generated");
        });

        // [2] ObservableEvents demo: static object's event (hand-written add/remove)
        WeakReferenceEventHub.Default.Subscribe<bool, bool>(
            holder: this,
            addHandler: h => MySingletonObject.Instance.MyEvent += h,
            removeHandler: h => MySingletonObject.Instance.MyEvent -= h,
            onNext: _ =>
            {
                Debug.WriteLine("Raised - MySingletonObject.Instance.MyEvent += h");
            }
        );

        // [3] ObservableEvents demo: static object's event (use reflection)
        WeakReferenceEventHub.Default.Subscribe<bool>(
            holder: this,
            target: MySingletonObject.Instance,
            eventName: nameof(MySingletonObject.MyEvent),
            onNext: _ =>
            {
                Debug.WriteLine("Raised - MySingletonObject.Instance.MyEvent reflection");
            }
        );

        // -------------------

        // Interactions demo: View registers handler; ViewModel triggers the interaction.
        _confirmInteractionRegistration = ViewModel.ConfirmAction.RegisterHandler(context =>
        {
            MessageBoxResult result = MessageBox.Show(
                context.Input,
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            context.SetOutput(result == MessageBoxResult.Yes);
        });
    }
}

public static class MyStaticObject
{
    public static event Action<bool>? MyStaticEvent;

    public static void RaiseSomeEvent(bool value)
    {
        MyStaticEvent?.Invoke(value);
    }
}

public class MySingletonObject
{
    public static MySingletonObject Instance { get; } = new();

    public event Action<bool>? MyEvent;

    public void RaiseSomeEvent(bool value)
    {
        MyEvent?.Invoke(value);
    }
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Interaction demo: ViewModel raises a confirmation request; View (MainWindow) responds via MessageBox.
    /// </summary>
    public Interaction<string, bool> ConfirmAction { get; } = new();

    [ObservableProperty]
    public partial string LastResult { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool StaticEventToggle { get; set; } = false;

    [RelayCommand]
    private async Task AskConfirmAsync()
    {
        // ViewModel calls Handle; the handler is registered by View and shows MessageBox, then returns user choice.
        var approved = await ConfirmAction.Handle("Do you want to continue?").ToTask();
        LastResult = approved ? "User chose: Yes" : "User chose: No";
    }

    /// <summary>
    /// Raises both static event and static-object event with an alternating bool (true/false).
    /// </summary>
    [RelayCommand]
    private void RaiseStaticEvents()
    {
        StaticEventToggle = !StaticEventToggle;
        MyStaticObject.RaiseSomeEvent(StaticEventToggle);
        MySingletonObject.Instance.RaiseSomeEvent(StaticEventToggle);
    }

    public void Dispose()
    {
    }
}
