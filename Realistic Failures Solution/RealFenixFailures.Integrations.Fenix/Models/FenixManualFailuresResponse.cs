using System.Text.Json.Serialization;

namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixManualFailuresResponse(
    [property: JsonPropertyName("atas")] IReadOnlyList<FenixAtaBlock> Atas
);
