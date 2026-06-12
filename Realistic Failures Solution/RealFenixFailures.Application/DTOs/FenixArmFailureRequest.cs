namespace RealFenixFailures.Application.DTOs;

public sealed record FenixArmFailureRequest(
    string Id,
    bool Failed,
    FenixArmFailureConditionRequest? FailureCondition
);
public sealed record FenixArmFailureConditionRequest {
    public int? Ias { get; set; }
    public int? Alt { get; set; }
    public int? Altb { get; set; }
    public int? Time { get; set; }
    public string? AfterEvent { get; set; }
    public int? AfterEventSeconds { get; set; }
};