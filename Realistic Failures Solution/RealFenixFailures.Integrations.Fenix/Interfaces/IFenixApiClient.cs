using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Interfaces;

public interface IFenixApiClient {
    Task<FenixManualFailuresResponse?> GetManualFailuresAsync(CancellationToken cancellationToken);
    Task SetManualFailureAsync(FenixSaveManualRequest rq, CancellationToken cancellationToken);
}
