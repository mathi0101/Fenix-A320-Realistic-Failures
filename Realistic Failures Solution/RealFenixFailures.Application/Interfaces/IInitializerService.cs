namespace RealFenixFailures.Application.Interfaces;

public interface IInitializerService {
    Task InitializeAsync(CancellationToken ct);
}