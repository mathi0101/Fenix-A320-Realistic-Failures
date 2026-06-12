namespace RealFenixFailures.Integrations.Fenix.Models;

public sealed record FenixFailureConditionRequest {
    public int? Ias { get; set; }
    public int? Alt { get; set; }
    public int? Altb { get; set; }
    public int? Time { get; set; }
    public string? AfterEvent { get; set; }
    public int? AfterEventSeconds { get; set; }
};

