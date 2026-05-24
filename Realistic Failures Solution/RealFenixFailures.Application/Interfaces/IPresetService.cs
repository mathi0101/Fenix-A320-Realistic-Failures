using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IPresetService {

    Task InitializeAsync(CancellationToken ct);
    Task DeletePresetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PresetDto>> GetTrainingPresetsAsync(CancellationToken ct);
    Task<IReadOnlyList<PresetDto>> GetRealisticPresetsAsync(CancellationToken ct);
    Task<IReadOnlyList<PresetDto>> GetCustomPresetsAsync(CancellationToken ct);
    Task<PresetDto?> GetByIdAsync(int presetId, CancellationToken cancellationToken);
}
