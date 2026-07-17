using RealFenixFailures.Application.DTOs;

namespace RealFenixFailures.Application.Interfaces;

public interface IUserAircraftService {
    /// <summary>Devuelve todas las aeronaves del usuario (Paso 1).</summary>
    Task<IReadOnlyList<UserAircraftDto>> GetAllAsync(CancellationToken ct);

    /// <summary>Obtiene una aeronave por Id.</summary>
    Task<UserAircraftDto?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>Crea una nueva aeronave y la inicializa con desgaste 0 en todos los sistemas.</summary>
    Task<UserAircraftDto> CreateAsync(CreateUserAircraftRequest request, CancellationToken ct);

    /// <summary>Elimina una aeronave (y su desgaste asociado).</summary>
    Task DeleteAsync(int id, CancellationToken ct);

    /// <summary>Lee el desgaste actual de todos los sistemas de una aeronave (Paso 2).</summary>
    Task<IReadOnlyList<AircraftSystemWearDto>> GetSystemWearsAsync(int userAircraftId, CancellationToken ct);

    /// <summary>Historial de sesiones de vuelo de una aeronave, con sus fallas (Paso 2).</summary>
    Task<IReadOnlyList<FlightSessionDto>> GetAircraftSessionsAsync(int userAircraftId, CancellationToken ct);

    /// <summary>Datos agregados para el dashboard del Paso 2.</summary>
    Task<AircraftDashboardDto> GetDashboardAsync(int userAircraftId, CancellationToken ct);
}
