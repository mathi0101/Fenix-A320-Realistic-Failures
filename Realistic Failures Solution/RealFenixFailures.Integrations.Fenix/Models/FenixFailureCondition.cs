namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixFailureCondition(
    int? Id,
    int? Ias,
    int? Alt,
    int? Altb,
    int? Time,
    string? AfterEvent,
    int? AfterEventSeconds
    );
