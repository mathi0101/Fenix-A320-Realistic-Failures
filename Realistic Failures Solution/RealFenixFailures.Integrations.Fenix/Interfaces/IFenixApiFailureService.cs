using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix;

public interface IFenixApiFailureService {
    Task<bool> IsApiAvailableAsync(CancellationToken ct);
    Task<AllFenixFailuresResponseDto> GetAllFailuresAsync(CancellationToken ct);
    Task<bool> ArmFailureAsync(FenixSaveManualRequest def, CancellationToken ct);
    Task<bool> ResetAllFailuresAsync(CancellationToken ct);
}
