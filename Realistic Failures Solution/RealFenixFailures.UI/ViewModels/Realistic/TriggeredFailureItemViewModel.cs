using RealFenixFailures.Application.DTOs;
using RealFenixFailures.UI.ViewModels.Base;

namespace RealFenixFailures.UI.ViewModels.Realistic;

/// <summary>Falla disparada mostrada en el resumen expandido de una sesión (Paso 2).</summary>
public sealed class TriggeredFailureItemViewModel : ObservableObject {
    public TriggeredFailureItemViewModel(TriggeredFailureDto dto) {
        FailureName = dto.FailureName;
        FlightPhaseName = string.IsNullOrWhiteSpace(dto.FlightPhaseName)
            ? dto.FlightPhase.ToString()
            : dto.FlightPhaseName;
        TriggeredAt = dto.TriggeredAt;
    }

    public string FailureName { get; }
    public string FlightPhaseName { get; }
    public DateTime TriggeredAt { get; }

    public string TimeDisplay => TriggeredAt.ToLocalTime().ToString("HH:mm:ss");
}
