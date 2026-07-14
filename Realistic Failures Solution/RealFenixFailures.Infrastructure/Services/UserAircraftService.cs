using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Application.DTOs;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Infrastructure.Persistence;

namespace RealFenixFailures.Infrastructure.Services;

public class UserAircraftService : IUserAircraftService {
    private readonly RealFenixDbContext _dbContext;

    public UserAircraftService(RealFenixDbContext dbContext) {
        _dbContext = dbContext;
    }

    // -------------------------------------------------------------------------
    // Create
    // -------------------------------------------------------------------------

    public async Task<UserAircraftDto> CreateAsync(CreateUserAircraftRequest request, CancellationToken ct) {
        var aircraft = new UserAircraft {
            Registration = request.Registration,
            IcaoTypeCode = request.IcaoTypeCode,
            CreatedAt = DateTime.UtcNow,
            TotalFlights = 0,
            TotalFlightHours = 0
        };

        _dbContext.UserAircrafts.Add(aircraft);
        await _dbContext.SaveChangesAsync(ct);

        return ToDto(aircraft);
    }

    // -------------------------------------------------------------------------
    // Delete
    // -------------------------------------------------------------------------

    public async Task DeleteAsync(int id, CancellationToken ct) {
        var aircraft = await _dbContext.UserAircrafts
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"UserAircraft {id} not found.");

        _dbContext.UserAircrafts.Remove(aircraft);
        await _dbContext.SaveChangesAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Get
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<UserAircraftDto>> GetAllAsync(CancellationToken ct) {
        return await _dbContext.UserAircrafts
            .Select(x => ToDto(x))
            .ToListAsync(ct);
    }

    public async Task<UserAircraftDto?> GetByIdAsync(int id, CancellationToken ct) {
        var aircraft = await _dbContext.UserAircrafts
            .Include(x => x.SystemWears)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return aircraft is null ? null : ToDto(aircraft);
    }

    public async Task<AircraftDashboardDto> GetDashboardAsync(int userAircraftId, CancellationToken ct) {
        var aircraft = await _dbContext.UserAircrafts
            .Include(x => x.SystemWears)
            .Include(x => x.FlightSessions)
                .ThenInclude(s => s.TriggeredFailures)
            .FirstOrDefaultAsync(x => x.Id == userAircraftId, ct)
            ?? throw new KeyNotFoundException($"UserAircraft {userAircraftId} not found.");

        var sessions = await GetSessionsAsync(userAircraftId, ct);

        return new AircraftDashboardDto {
            Aircraft = ToDto(aircraft),
            SystemWears = aircraft.SystemWears
                .Select(ToSystemWearDto)
                .ToList(),
            Sessions = sessions,
            TotalFailuresTriggered = aircraft.FlightSessions
                .SelectMany(s => s.TriggeredFailures)
                .Count()
        };
    }

    // -------------------------------------------------------------------------
    // Sessions
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<FlightSessionDto>> GetSessionsAsync(int userAircraftId, CancellationToken ct) {
        return await _dbContext.FlightSessions
            .Where(s => s.UserAircraftId == userAircraftId)
            .OrderByDescending(s => s.StartedAt)
            .Include(s => s.TriggeredFailures)
            .Select(s => new FlightSessionDto {
                Id = s.Id,
                TriggeredFailures = s.TriggeredFailures
                    .Select(f => new TriggeredFailureDto {
                        Id = f.Id,
                        FenixFailureId = f.FenixFailureId,
                        FailureName = f.FenixFailure!.Name,
                        TriggeredAt = f.TriggeredAt,
                        FlightPhase = (int)f.FlightPhase
                    })
                    .ToList()
            })
            .ToListAsync(ct);
    }

    // -------------------------------------------------------------------------
    // System Wears
    // -------------------------------------------------------------------------

    public async Task<IReadOnlyList<AircraftSystemWearDto>> GetSystemWearsAsync(int userAircraftId, CancellationToken ct) {
        return await _dbContext.AircraftSystemWears
            .Where(w => w.UserAircraftId == userAircraftId)
            .Select(x => ToSystemWearDto(x))
            .ToListAsync(ct);
    }

    // -------------------------------------------------------------------------
    // Mappers
    // -------------------------------------------------------------------------

    private static UserAircraftDto ToDto(UserAircraft aircraft) => new() {
        Id = aircraft.Id,
        Registration = aircraft.Registration,
        IcaoTypeCode = aircraft.IcaoTypeCode,
        TotalFlightHours = aircraft.TotalFlightHours,
        TotalFlights = aircraft.TotalFlights,
        CreatedAt = aircraft.CreatedAt
    };

    private static AircraftSystemWearDto ToSystemWearDto(AircraftSystemWear w) => new() {
        WearableSystemId = w.Id,
        UserAircraftId = w.UserAircraftId,
        WearPercentage = w.WearPercentage,
        LastUpdatedAt = w.LastUpdatedAt,
        SystemName = w.WearableSystem!.Name,
        ShortName = w.WearableSystem!.ShortName,
        DisplayOrder = w.WearableSystem!.DisplayOrder
    };
}