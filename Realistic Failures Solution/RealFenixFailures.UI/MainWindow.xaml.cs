using RealFenixFailures.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace RealFenixFailures.UI;

public partial class MainWindow : Window {
    private readonly ViewModels.MainWindow _viewModel;
    public MainWindow(ViewModels.MainWindow viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (e.ClickCount == 2)
            MaximizeWindow(sender, e);
        else
            DragMove();
    }

    private void MinimizeWindow(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaximizeWindow(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void CloseWindow(object sender, RoutedEventArgs e) =>
        Close();
}