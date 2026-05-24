using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Services;

namespace RealFenixFailures.Application.Services;

public class InitializerService : IInitializerService {
    private readonly IFailuresPersistenceService _failuresPersistenceService;
    private readonly IPresetService _presetService;

    public InitializerService(IFailuresPersistenceService failuresPersistenceService, IPresetService presetsLoader) {
        _failuresPersistenceService = failuresPersistenceService;
        _presetService = presetsLoader;
    }

    public async Task InitializeAsync(CancellationToken ct) {
        await _failuresPersistenceService.LoadInitialFailuresAsync(ct);
        await _presetService.InitializeAsync(ct);
    }
}
