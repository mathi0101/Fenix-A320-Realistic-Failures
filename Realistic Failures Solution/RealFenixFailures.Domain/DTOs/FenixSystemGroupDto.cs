namespace RealFenixFailures.Domain.DTOs;

public sealed record FenixSystemGroupDto(
    string Name,
    IReadOnlyList<FenixFailureDto> Failures
);