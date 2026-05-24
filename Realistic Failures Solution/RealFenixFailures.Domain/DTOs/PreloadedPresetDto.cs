using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.DTOs;

internal record PreloadedPresetDto(
    string Name,
    string Description,
    string TriggerDescription,
    FlightPhaseEnum Phase,
    DifficultyEnum Difficulty,
    ICollection<PresetFailureDefinition> Failures
);
