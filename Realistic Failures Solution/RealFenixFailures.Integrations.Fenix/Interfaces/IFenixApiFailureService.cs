using RealFenixFailures.Domain.DTOs;
using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix;

public interface IFenixApiFailureService {
    Task<bool> IsApiAvailableAsync(CancellationToken ct);
    Task<AllFenixFailuresResponseDto> GetAllFailuresAsync(CancellationToken ct);
    Task SetFailureAsync(string fenixId, bool failed, CancellationToken ct);
    Task ArmFailureAsync(FenixSaveManualRequest def, CancellationToken ct);
    Task ResetAllFailuresAsync(CancellationToken ct);
}
