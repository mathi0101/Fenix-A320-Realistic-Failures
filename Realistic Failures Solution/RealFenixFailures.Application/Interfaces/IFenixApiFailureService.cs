using RealFenixFailures.Domain.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IFenixApiFailureService {
    Task<AllFenixFailuresResponseDto> GetAllFailuresAsync(CancellationToken cancellationToken);
    Task SetFailureAsync(string failureId, bool failed, CancellationToken cancellationToken);
    Task ResetAllFailuresAsync(CancellationToken cancellationToken);
    Task<bool> IsApiAvailableAsync(CancellationToken cancellationToken);
}
