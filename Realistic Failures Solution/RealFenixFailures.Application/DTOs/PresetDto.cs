using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public record PresetDto {
    public required int Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public PresetTypeEnum PresetType { get; init; }
    public required FlightPhaseEnum Phase { get; init; }
    public required DifficultyEnum Difficulty { get; init; }
    public string TriggerDescription { get; init; } = string.Empty;

    public IReadOnlyList<PresetFailureDto> Failures { get; init; } = new List<PresetFailureDto>();
}