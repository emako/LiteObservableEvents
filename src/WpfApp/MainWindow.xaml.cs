using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace WpfApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ThemeMode = ThemeMode.Dark;
        DataContext = new MainViewModel();
        InitializeComponent();
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    public void Dispose()
    {
    }
}
