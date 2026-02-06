using CommunityToolkit.Mvvm.ComponentModel;
using LiteObservableEvents;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;

namespace WpfApp;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        ThemeMode = ThemeMode.Dark;
        DataContext = ViewModel = new();
        InitializeComponent();

        this.Events().Closed
            .Subscribe(_ =>
            {
                WeakReferenceEventHub.Default.UnsubscribeAllOf(this);
                ViewModel?.Dispose();
            });

        WeakReferenceEventHub.Default.Subscribe(this, ViewModel.Events().SomeEvent, _ =>
        {
            Debug.WriteLine("Window Loaded!");
        });
    }
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    public event EventHandler? SomeEvent;

    public void Dispose()
    {
    }
}
