using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

public sealed record FailureTriggerLogDto(
    DateTimeOffset TriggeredAtUtc,
    string FailureId,
    string FailureName,
    ComplexFlightPhaseEnum FlightPhase,
    string PresetName
);
