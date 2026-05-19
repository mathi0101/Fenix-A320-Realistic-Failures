using RealFenixFailures.UI.ViewModels;
using System.Windows;

namespace RealFenixFailures.UI;

public partial class MainWindow : Window {
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel) {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.InitializeAsync();
    }
}