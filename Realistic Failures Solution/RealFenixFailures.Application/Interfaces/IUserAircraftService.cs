using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Application.Interfaces;

public interface IUserAircraftService {
    Task<UserAircraft> GetOrCreateDefaultAsync(CancellationToken ct);
    Task<IReadOnlyList<AircraftSystemWear>> GetSystemWearsAsync(int userAircraftId, CancellationToken ct);
}
