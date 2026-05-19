using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Infrastructure.Persistence;

namespace RealFenixFailures.Infrastructure.Repositories;

public class FlightSessionRepository : IFlightSessionRepository
{
    private readonly RealFenixDbContext _dbContext;

    public FlightSessionRepository(RealFenixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FlightSession> AddAsync(FlightSession session, CancellationToken cancellationToken)
    {
        var result = await _dbContext.FlightSessions.AddAsync(session, cancellationToken);
        return result.Entity;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
