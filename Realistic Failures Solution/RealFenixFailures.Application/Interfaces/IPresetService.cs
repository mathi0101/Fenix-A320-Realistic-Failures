using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IPresetService {

    Task InitializeAsync(CancellationToken ct);
    Task DeletePresetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<FailurePreset>> GetTrainingPresetsAsync(CancellationToken ct);
    Task<IReadOnlyList<FailurePreset>> GetRealisticPresetsAsync(CancellationToken ct);
    Task<IReadOnlyList<FailurePreset>> GetCustomPresetsAsync(CancellationToken ct);
    Task<FailurePreset?> GetByIdAsync(int presetId, CancellationToken ct);
    Task<FailurePreset> CreateEmptyCustomPresetAsync(CancellationToken ct);
}
