namespace RealFenixFailures.Domain.Services;

public interface IFailuresPersistenceService {
    Task LoadInitialFailuresAsync(CancellationToken ct);
}
