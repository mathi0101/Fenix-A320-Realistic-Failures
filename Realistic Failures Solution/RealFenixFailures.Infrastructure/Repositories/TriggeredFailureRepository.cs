using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Infrastructure.Persistence;

namespace RealFenixFailures.Infrastructure.Repositories;

public class TriggeredFailureRepository : ITriggeredFailureRepository
{
    private readonly RealFenixDbContext _dbContext;

    public TriggeredFailureRepository(RealFenixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TriggeredFailure> AddAsync(TriggeredFailure triggeredFailure, CancellationToken cancellationToken)
    {
        var result = await _dbContext.TriggeredFailures.AddAsync(triggeredFailure, cancellationToken);
        return result.Entity;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
