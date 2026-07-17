using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class FailurePreset {

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TriggerDescription { get; set; } = string.Empty;
    public int PresetTypeId { get; set; }
    public PresetTypeEnum PresetType {
        get => (PresetTypeEnum)PresetTypeId;
        set => PresetTypeId = (int)value;
    }
    public ComplexFlightPhaseEnum FlightPhase { get; set; }
    public DifficultyEnum Difficulty { get; set; }

    public ICollection<PresetFailureDefinition> PresetFailureDefinitions { get; set; } = new List<PresetFailureDefinition>();
}
