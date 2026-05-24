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
    }
}