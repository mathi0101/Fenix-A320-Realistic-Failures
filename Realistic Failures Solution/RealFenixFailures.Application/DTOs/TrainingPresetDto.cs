using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public class TrainingPresetDto {
    public required int Id { get; init; }
    public required string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public required FlightPhase Phase { get; init; }
    public required TrainingScenarioDifficultyEnum Difficulty { get; init; }
    public string TriggerDescription { get; init; } = string.Empty;
}