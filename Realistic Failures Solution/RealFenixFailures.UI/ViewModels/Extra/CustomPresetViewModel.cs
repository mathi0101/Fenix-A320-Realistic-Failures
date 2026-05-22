using RealFenixFailures.UI.ViewModels.Base;

namespace RealFenixFailures.UI.ViewModels.Extra;

public class CustomPresetViewModel : ObservableObject {
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int FailureCount { get; init; }
}
