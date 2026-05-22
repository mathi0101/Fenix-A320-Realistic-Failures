using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface ITrainingPresetService {
    Task<IReadOnlyList<TrainingPresetDto>> GetTrainingPresetsAsync(CancellationToken cancellationToken);
}
