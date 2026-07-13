using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RealFenixFailures.Infrastructure.Services;

public class UserAircraftService : IUserAircraftService {
    private readonly RealFenixDbContext _dbContext;

    public UserAircraftService(RealFenixDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<UserAircraft> GetOrCreateDefaultAsync(CancellationToken ct) {
        var defaultAircraft = await _dbContext.UserAircrafts
            .FirstOrDefaultAsync(ct);

        if (defaultAircraft != null) {
            return defaultAircraft;
        }

        defaultAircraft = new UserAircraft {
            Registration = "N1000RF",
            IcaoTypeCode = "A320",
            TotalFlightHours = 0,
            TotalFlights = 0,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _dbContext.UserAircrafts.Add(defaultAircraft);
        await _dbContext.SaveChangesAsync(ct);

        return defaultAircraft;
    }

    public async Task<IReadOnlyList<AircraftSystemWear>> GetSystemWearsAsync(int userAircraftId, CancellationToken ct) {
        return await _dbContext.AircraftSystemWears
            .Where(w => w.UserAircraftId == userAircraftId)
            .ToListAsync(ct);
    }
}
