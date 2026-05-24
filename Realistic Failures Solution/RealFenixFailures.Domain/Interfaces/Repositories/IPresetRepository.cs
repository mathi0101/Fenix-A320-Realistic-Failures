using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Domain.Interfaces.Repositories;

public interface IPresetRepository {
    Task AddAsync(IReadOnlyList<FailurePreset> presets, CancellationToken ct);
    Task<IReadOnlyList<FailurePreset>> GetAllAsync(PresetTypeEnum presetType, CancellationToken cancellationToken);
    Task<bool> GetAnyAsync(PresetTypeEnum presetType, CancellationToken ct);
    Task<FailurePreset?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
