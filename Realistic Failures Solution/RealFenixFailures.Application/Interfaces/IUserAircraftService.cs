using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IUserAircraftService {
    Task<UserAircraft?> GetAircraftById(int userAircraftId, CancellationToken ct);
    Task<UserAircraft> GetOrCreateDefaultAsync(CancellationToken ct);
    Task<IReadOnlyList<AircraftSystemWear>> GetSystemWearsAsync(int userAircraftId, CancellationToken ct);
}
