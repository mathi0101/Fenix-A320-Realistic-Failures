namespace RealFenixFailures.Application.DTOs;

public sealed class AllFenixFailuresResponseDto {
    public IReadOnlyList<FenixMajorSystemGroupDto> MajorGroups { get; set; } = new List<FenixMajorSystemGroupDto>();

    public IReadOnlyList<FenixFailureDto> GetAllFailures() => MajorGroups.SelectMany(mj => mj.SystemGroups ?? Array.Empty<FenixSystemGroupDto>())
            .SelectMany(group => group.Failures ?? Array.Empty<FenixFailureDto>())
            .ToList();

}
