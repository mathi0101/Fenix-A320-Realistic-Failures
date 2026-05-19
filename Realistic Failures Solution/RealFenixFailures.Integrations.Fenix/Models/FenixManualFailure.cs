using System.Text.Json;

namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixManualFailure(
    string Id,
    string Title,
    JsonElement? FailureCondition,
    bool Failed
);
