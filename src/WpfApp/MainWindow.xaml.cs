using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using LiteObservableEvents;

namespace WpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ThemeMode = ThemeMode.Dark;
        DataContext = new MainViewModel();
        InitializeComponent();

        // Example: Use LiteObservableEvents to subscribe to window events
        this.Events().Closed
            .Subscribe(_ => (DataContext as IDisposable)?.Dispose());

        this.Events().Loaded
            .Subscribe(_ => System.Diagnostics.Debug.WriteLine("Window Loaded!"));
    }
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    public void Dispose()
    {
    }
}
