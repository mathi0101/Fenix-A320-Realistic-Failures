using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class FenixFailureDefinition {
    public int Id { get; set; }
    public string FenixFailureId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public FenixFailureGroup Group { get; set; }
    public FailureSeverity Severity { get; set; }

    public ICollection<FailurePreset> Presets { get; set; } = new List<FailurePreset>();
}
