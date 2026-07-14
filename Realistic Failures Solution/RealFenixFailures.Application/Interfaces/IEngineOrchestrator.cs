using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Domain.Enums;
using System.ComponentModel;

namespace RealFenixFailures.Application.Interfaces;

public interface IEngineOrchestrator : INotifyPropertyChanged {
    bool IsEngineActive { get; }
    UserAppMode CurrentMode { get; }

    ConnectionStatusDto ConnectionStatus { get; }

    /// Metodos para controlar la actualizacion automática
    Task StartAutomaticTimerAsync(CancellationToken ct);
    Task UpdateConnection(CancellationToken ct);
    Task StopAutomaticTimerAsync(CancellationToken ct);
    bool IsTimerRunning { get; }

    // Métodos para controlar el motor de fallas
    Task StartRealisticModeAsync(int aircraftId, RiskLevel risk, CancellationToken ct);
    Task StartTrainingPresetAsync(int presetId, CancellationToken ct);
    Task StartCustomModeAsync(int presetId, bool activateImmediately, CancellationToken ct);

    Task StopCurrentModeAsync(CancellationToken ct);

    // Método para obtener el historial de fallas recientes
    Task<List<FailureTriggerLogDto>> GetRecentFailuresAsync(CancellationToken ct);
}