using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Mappers;

internal static class FenixMappers {
    internal static AllFenixFailuresResponseDto FenixJsonFailuresToDto(FenixManualFailuresResponse response) {
        return new AllFenixFailuresResponseDto {
            MajorGroups =
            [..response.Atas.Select(
                block => new FenixMajorSystemGroupDto(block.Title, block.ShortTitle, block.Groups.Select(
                    group => new FenixSystemGroupDto(group.GroupName, group.Failures.Select(
                        failure => new FenixFailureDto(failure.Id, failure.Title, failure.Failed,
                            failure.FailureCondition is FenixFailureCondition fc ?
                            new FenixFailureConditionDto(fc.Id, fc.Ias, fc.Alt, fc.Altb, fc.Time, fc.AfterEvent, fc.AfterEventSeconds) : null )
                        ).ToList())
                    ).ToList())
                )]
        };
    }
}
