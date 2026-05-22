namespace RealFenixFailures.Application.DTOs;

public sealed record FenixFailureDto(
    string FenixId,
    string Description,
    bool Failed,
    FenixFailureConditionDto? FailureCondition
);
