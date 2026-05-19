using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public sealed record FailureTriggerLogDto(
    DateTimeOffset TriggeredAtUtc,
    string FailureName,
    FlightPhase FlightPhase,
    string PresetName
);
