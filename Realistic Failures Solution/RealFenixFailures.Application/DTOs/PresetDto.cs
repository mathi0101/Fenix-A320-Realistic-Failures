using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public sealed record PresetDto(
    int Id,
    string Name,
    string Description,
    PresetType PresetType,
    int FailureCount
);
