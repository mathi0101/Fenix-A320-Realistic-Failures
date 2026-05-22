using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Infrastructure.Persistence;

namespace RealFenixFailures.Infrastructure.Repositories;

public class FailurePresetRepository : IFailurePresetRepository {
    private readonly RealFenixDbContext _dbContext;

    public FailurePresetRepository(RealFenixDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FailurePreset>> GetAllAsync(CancellationToken cancellationToken) {
        return await _dbContext.FailurePresets
            .Include(x => x.FailureDefinitions)
            .OrderBy(x => x.PresetType)
            .ToListAsync(cancellationToken);
    }

    public async Task<FailurePreset?> GetByIdAsync(int id, CancellationToken cancellationToken) {
        return await _dbContext.FailurePresets
            .Include(x => x.FailureDefinitions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
