using System.Text.Json;

namespace RealFenixFailures.Domain.DTOs;

public class JsonPreloadedPresetDto {
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string TriggerDescription { get; set; } = "";

    // Puede venir como número o como string (nombre del enum)
    public JsonElement Phase { get; set; }

    public JsonElement Difficulty { get; set; }

    public List<JsonPresetFailureDto> Failures { get; set; } = new();
}

public class JsonPresetFailureDto {
    public string FenixFailureId { get; set; } = "";
    public int? ProbabilityGroup { get; set; }
    public double Probability { get; set; }
    public string? Ias { get; set; }
    public string? Above_Altitude { get; set; }
    public string? Below_Altitude { get; set; }
    public string? AfterEventSeconds { get; set; }
    public string? AfterEvent { get; set; }
}