using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Application.Mappers;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;

namespace RealFenixFailures.Application.Services;

public class PresetService : IPresetService {
    private readonly IPresetRepository _presetRepository;
    private readonly IPresetsLoader _presetsLoader;

    public PresetService(IPresetRepository presetRepository, IPresetsLoader presetsLoader) {
        _presetRepository = presetRepository;
        _presetsLoader = presetsLoader;
    }

    public async Task InitializeAsync(CancellationToken ct) {
        var hasTrainingPresets = await _presetRepository.GetAnyAsync(PresetTypeEnum.TrainingMode, ct);
        if (!hasTrainingPresets) {
            var presets = await _presetsLoader.GetTrainingPresetsAsync(ct);
            await _presetRepository.AddAsync(presets, ct);
        }
    }

    public Task DeletePresetAsync(int id, CancellationToken ct) {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PresetDto>> GetTrainingPresetsAsync(CancellationToken ct) {
        var presets = await _presetRepository.GetAllAsync(PresetTypeEnum.TrainingMode, ct);
        return presets
            .Select(p => p.ToDto())
            .ToList();
    }

    public async Task<IReadOnlyList<PresetDto>> GetRealisticPresetsAsync(CancellationToken ct) {
        var presets = await _presetRepository.GetAllAsync(PresetTypeEnum.RealisticMode, ct);
        return presets
            .Select(p => p.ToDto())
            .ToList();
    }

    public async Task<IReadOnlyList<PresetDto>> GetCustomPresetsAsync(CancellationToken ct) {
        var presets = await _presetRepository.GetAllAsync(PresetTypeEnum.Custom, ct);
        return presets
            .Select(p => p.ToDto())
            .ToList();
    }

    public async Task<PresetDto?> GetByIdAsync(int presetId, CancellationToken ct) {
        var preset = await _presetRepository.GetByIdAsync(presetId, ct);
        return preset?.ToDto();
    }
}
