using RealFenixFailures.UI.ViewModels.Base;

namespace RealFenixFailures.UI.ViewModels.Extra;


public class TrainingScenarioViewModel : ObservableObject {
    private bool _isSelected;

    public required int Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Description { get; init; } = string.Empty;
    public required string Phase { get; init; } = string.Empty;
    public required string Difficulty { get; init; } = string.Empty;
    public string TriggerDescription { get; init; } = string.Empty;

    public bool IsSelected {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
