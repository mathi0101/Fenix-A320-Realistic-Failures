using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Entities;

public class FailurePreset
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PresetType PresetType { get; set; }

    public ICollection<FailureDefinition> FailureDefinitions { get; set; } = new List<FailureDefinition>();
}
