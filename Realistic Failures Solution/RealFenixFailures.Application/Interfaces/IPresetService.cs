using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IPresetService {
    Task CreateEmptyCustomPresetAsync(CancellationToken none);
    Task DeletePresetAsync(int id, CancellationToken none);
    Task<IReadOnlyList<PresetDto>> GetPresetsAsync(CancellationToken cancellationToken);
}
