using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.DTOs;

internal record PreloadedPresetDto(
    string Name,
    string Description,
    string TriggerDescription,
    ComplexFlightPhaseEnum Phase,
    DifficultyEnum Difficulty,
    ICollection<PresetFailureDefinition> Failures
);
