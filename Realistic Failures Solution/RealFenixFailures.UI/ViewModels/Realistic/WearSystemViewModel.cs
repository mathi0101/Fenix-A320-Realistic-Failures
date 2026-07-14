using System.Windows.Media;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.UI.ViewModels.Base;

namespace RealFenixFailures.UI.ViewModels.Realistic;

/// <summary>Barra de desgaste de un sistema (Paso 2) con color dinámico.</summary>
public sealed class WearSystemViewModel : ObservableObject {
    public WearSystemViewModel(AircraftSystemWearDto dto) {
        WearableSystemId = dto.WearableSystemId;
        Name = dto.SystemName;
        ShortName = dto.ShortName;
        DisplayOrder = dto.DisplayOrder;
        WearPercentage = (int)System.Math.Round(dto.WearPercentage);
    }

    public int WearableSystemId { get; }
    public string Name { get; }
    public string ShortName { get; }
    public int DisplayOrder { get; }
    public int WearPercentage { get; }

    public string PercentDisplay => $"{WearPercentage}%";

    // Verde 0–40, Amarillo/naranja 40–70, Rojo 70–100
    public Color WearColor => WearPercentage switch {
        < 40 => Color.FromRgb(0x4C, 0xAF, 0x50),  // #4CAF50 verde
        < 70 => Color.FromRgb(0xF5, 0x9E, 0x0B),  // ámbar
        _ => Color.FromRgb(0xEF, 0x44, 0x44)      // rojo
    };

    public Brush WearBrush => new SolidColorBrush(WearColor);
}
