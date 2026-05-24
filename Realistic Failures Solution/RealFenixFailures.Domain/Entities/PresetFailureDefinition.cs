namespace RealFenixFailures.Domain.Entities;

public class PresetFailureDefinition {
    public int PresetId { get; set; }
    public required string FenixFailureId { get; set; } = string.Empty;
    public int? ProbabilityGroup { get; set; }
    public required double Probability { get; set; }
    public string? Ias { get; set; }
    public string? Above_Altitude { get; set; }
    public string? Below_Altitude { get; set; }
    public string? Time { get; set; }
    public string? AfterEvent { get; set; }
    public string? AfterEventSeconds { get; set; }

    public FenixFailureDefinition? FenixFailure { get; set; }
    public FailurePreset? Preset { get; set; }
}
