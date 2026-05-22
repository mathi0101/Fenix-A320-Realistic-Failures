namespace RealFenixFailures.Application.DTOs;

public sealed record FenixMajorSystemGroupDto(
    string Title,
    string ShortTitle,
    IReadOnlyList<FenixSystemGroupDto> SystemGroups
);