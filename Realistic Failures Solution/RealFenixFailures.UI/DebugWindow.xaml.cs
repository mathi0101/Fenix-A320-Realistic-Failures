using RealFenixFailures.UI.ViewModels;
using System.Windows;

namespace RealFenixFailures.UI;

public partial class DebugWindow : Window {
    private readonly DebugViewModel _viewModel;

    public DebugWindow(DebugViewModel viewModel) {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += async (_, _) => await _viewModel.InitializeAsync();
    }
}