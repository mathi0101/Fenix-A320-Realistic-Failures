// ============================================================================
//  DTOs para el flujo del Modo Realista (selección de aeronave, dashboard,
//  desgaste, historial de sesiones y fallas disparadas).
// ============================================================================
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Application.DTOs;

/// <summary>Aeronave del usuario para listar/seleccionar en el Paso 1.</summary>
public sealed record UserAircraftDto {
    public int Id { get; init; }
    public string Registration { get; init; } = string.Empty;
    public string IcaoTypeCode { get; init; } = string.Empty;
    public double TotalFlightHours { get; init; }
    public int TotalFlights { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<AircraftSystemWearDto> SystemsWear { get; init; } = [];
    public IReadOnlyList<FlightSessionDto> FlightSessions { get; init; } = [];
}

/// <summary>Payload para crear una nueva aeronave (Paso 1 - formulario inline).</summary>
public sealed record CreateUserAircraftRequest {
    public string Registration { get; init; } = string.Empty;
    public string IcaoTypeCode { get; init; } = string.Empty;
}

/// <summary>Desgaste de un sistema para el Paso 2 (dashboard).</summary>
public sealed record AircraftSystemWearDto {
    public required SystemWearDto SystemWear { get; init; }
    public int UserAircraftId { get; init; }
    /// <summary>Porcentaje de desgaste 0–100.</summary>
    public double WearPercentage { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}
public sealed record SystemWearDto {
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>Falla disparada dentro de una sesión (resumen expandido del Paso 2).</summary>
public sealed record TriggeredFailureDto {
    public required int Id { get; init; }
    public required string FenixFailureId { get; init; }
    public ComplexFlightPhaseEnum FlightPhase { get; init; }
    public string FailureName { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
}

/// <summary>Sesión de vuelo del historial (Paso 2), con sus fallas.</summary>
public sealed record FlightSessionDto {
    public int Id { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    /// <summary>1 = Bajo, 2 = Moderado, 3 = Alto.</summary>
    public RiskLevel RiskLevel { get; init; }
    public IReadOnlyList<TriggeredFailureDto> TriggeredFailures { get; init; } = Array.Empty<TriggeredFailureDto>();

    public int FailureCount => TriggeredFailures.Count;
    public TimeSpan? Duration => FinishedAt.HasValue ? FinishedAt.Value - StartedAt : null;
}

/// <summary>Datos agregados de la aeronave para el dashboard (Paso 2).</summary>
public sealed record AircraftDashboardDto {
    public UserAircraftDto Aircraft { get; init; } = new();
    public int TotalFailuresTriggered { get; init; }
    public IReadOnlyList<AircraftSystemWearDto> SystemWears { get; init; } = Array.Empty<AircraftSystemWearDto>();
}
