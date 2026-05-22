using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Domain.Interfaces.Repositories;

public interface IFailurePresetRepository {
    Task<IReadOnlyList<FailurePreset>> GetAllAsync(CancellationToken cancellationToken);
    Task<FailurePreset?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
