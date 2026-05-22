namespace RealFenixFailures.Application.Services;

public interface IFailurePersistenceService {
    Task InitializeAsync(CancellationToken ct);
}
