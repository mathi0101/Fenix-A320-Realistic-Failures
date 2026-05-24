using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Infrastructure.Persistence;

namespace RealFenixFailures.Infrastructure.Repositories;

public class PresetRepository : IPresetRepository {
    private readonly RealFenixDbContext _dbContext;

    public PresetRepository(RealFenixDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task AddAsync(IReadOnlyList<FailurePreset> presets, CancellationToken ct) {
        await _dbContext.AddRangeAsync(presets, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FailurePreset>> GetAllAsync(PresetTypeEnum presetType, CancellationToken ct) {
        return await _dbContext.FailurePresets
            .Where(x => x.PresetType == presetType)
            .ToListAsync(ct);
    }

    public async Task<bool> GetAnyAsync(PresetTypeEnum presetType, CancellationToken ct) {
        return await _dbContext.FailurePresets.AnyAsync(p => p.PresetType == presetType, ct);
    }

    public async Task<FailurePreset?> GetByIdAsync(int id, CancellationToken ct) {
        return await _dbContext.FailurePresets
            .Include(x => x.PresetFailureDefinitions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
