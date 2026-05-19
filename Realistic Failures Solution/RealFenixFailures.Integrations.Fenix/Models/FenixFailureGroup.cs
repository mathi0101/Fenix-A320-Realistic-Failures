namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixFailureGroup(
    string GroupName,
    IReadOnlyList<FenixManualFailure> Failures
);
