using RealFenixFailures.Integrations.Fenix.Models;

namespace RealFenixFailures.Integrations.Fenix.Interfaces;

public interface IFenixApiClient {
    Task<bool> IsApiAlive(CancellationToken ct);
    Task<Stream?> GetManualFailuresAsync(CancellationToken ct);
    Task<Stream?> SendFailureAsync(FenixSaveManualRequest rq, CancellationToken ct);
}
