namespace RealFenixFailures.Domain.DTOs;

public sealed class AllFenixFailuresResponseDto {
    public IReadOnlyList<FenixMajorSystemGroupDto> MajorGroups { get; set; } = new List<FenixMajorSystemGroupDto>();

    public IReadOnlyList<FenixFailureDto> GetFailuresList() => MajorGroups.SelectMany(mj => mj.SystemGroups ?? Array.Empty<FenixSystemGroupDto>())
            .SelectMany(group => group.Failures ?? Array.Empty<FenixFailureDto>())
            .ToList();

}
