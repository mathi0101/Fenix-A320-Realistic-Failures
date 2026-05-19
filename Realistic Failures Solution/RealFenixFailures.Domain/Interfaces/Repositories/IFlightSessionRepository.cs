using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Domain.Interfaces.Repositories;

public interface IFlightSessionRepository
{
    Task<FlightSession> AddAsync(FlightSession session, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
