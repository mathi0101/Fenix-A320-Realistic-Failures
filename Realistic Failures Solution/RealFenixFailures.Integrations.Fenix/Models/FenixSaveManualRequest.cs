namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixSaveManualRequest(
    string Id,
    bool Failed,
    FenixFailureConditionRequest? FailureCondition
);
