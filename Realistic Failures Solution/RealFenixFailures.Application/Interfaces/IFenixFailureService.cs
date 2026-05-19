using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixFailureService
{
    Task<IReadOnlyList<FenixFailureDto>> GetAvailableFailuresAsync(CancellationToken cancellationToken);
    Task SetFailureAsync(string failureId, bool failed, CancellationToken cancellationToken);
    Task ResetAllFailuresAsync(CancellationToken cancellationToken);
    Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken);
}
