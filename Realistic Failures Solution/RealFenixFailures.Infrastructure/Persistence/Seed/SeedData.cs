using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;
using RealFenixFailures.Domain.Enums;

namespace RealFenixFailures.Infrastructure.Persistence;

public static class SeedData {

    public static void Apply(ModelBuilder modelBuilder) {
        modelBuilder.Entity<PresetType>().HasData(
        Enum.GetValues(typeof(PresetTypeEnum))
            .Cast<PresetTypeEnum>()
            .Select(e => new PresetType {
                Id = (int)e,
                Description = e.ToString()
            })
        );

        SeedAircraftWearableSystems(modelBuilder);
    }

    private static void SeedAircraftWearableSystems(ModelBuilder modelBuilder) {
        var systems = new List<AircraftWearableSystem> {
            new() { Id = 1, Name = "Engine 1", ShortName = "ENG1", DisplayOrder = 1 },
            new() { Id = 2, Name = "Engine 2", ShortName = "ENG2", DisplayOrder = 2 },
            new() { Id = 3, Name = "Hydraulic System", ShortName = "HYD", DisplayOrder = 3 },
            new() { Id = 4, Name = "Landing Gear", ShortName = "GEAR", DisplayOrder = 4 },
            new() { Id = 5, Name = "Navigation Systems", ShortName = "NAV", DisplayOrder = 5 },
            new() { Id = 6, Name = "Pneumatic System", ShortName = "PNEU", DisplayOrder = 6 },
            new() { Id = 7, Name = "APU", ShortName = "APU", DisplayOrder = 7 },
            new() { Id = 8, Name = "Doors", ShortName = "DOOR", DisplayOrder = 8 }
        };

        modelBuilder.Entity<AircraftWearableSystem>().HasData(systems);
    }
}
