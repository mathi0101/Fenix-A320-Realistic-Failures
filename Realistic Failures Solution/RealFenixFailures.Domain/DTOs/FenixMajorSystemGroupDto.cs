namespace RealFenixFailures.Domain.DTOs;

public sealed record FenixMajorSystemGroupDto(
    string Title,
    string ShortTitle,
    IReadOnlyList<FenixSystemGroupDto> SystemGroups
);