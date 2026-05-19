using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;

public class PresetService : IPresetService {
    private readonly IFailurePresetRepository _presetRepository;

    public PresetService(IFailurePresetRepository presetRepository) {
        _presetRepository = presetRepository;
    }

    public Task CreateEmptyCustomPresetAsync(CancellationToken none) {
        throw new NotImplementedException();
    }

    public Task DeletePresetAsync(Guid id, CancellationToken none) {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PresetDto>> GetPresetsAsync(CancellationToken cancellationToken) {
        var presets = await _presetRepository.GetAllAsync(cancellationToken);

        return presets
            .Select(p => new PresetDto(p.Id, p.Name, p.Description, p.PresetType, p.FailureDefinitions.Count))
            .ToList();
    }
}
