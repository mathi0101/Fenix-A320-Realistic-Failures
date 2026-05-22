namespace RealFenixFailures.Application.DTOs;

public sealed record FenixFailureConditionDto(
    int Id,
    int Ias,
    int Alt,
    int Altb,
    int Time,
    string? AfterEvent,
    int? AfterEventSeconds
);
