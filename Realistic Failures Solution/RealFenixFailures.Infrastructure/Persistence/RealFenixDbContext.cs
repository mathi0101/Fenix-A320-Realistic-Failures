using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Infrastructure.Persistence;

public class RealFenixDbContext : DbContext {
    public RealFenixDbContext(DbContextOptions<RealFenixDbContext> options) : base(options) {
    }

    public DbSet<FenixFailureDefinition> FenixFailureDefinitions => Set<FenixFailureDefinition>();
    public DbSet<FenixFailureGroup> FenixFailureGroups => Set<FenixFailureGroup>();
    public DbSet<FenixFailureSystem> FenixFailureSystems => Set<FenixFailureSystem>();
    public DbSet<FailurePreset> FailurePresets => Set<FailurePreset>();
    public DbSet<FlightSession> FlightSessions => Set<FlightSession>();
    public DbSet<TriggeredFailure> TriggeredFailures => Set<TriggeredFailure>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        #region Fenix Failures

        modelBuilder.Entity<FenixFailureDefinition>(entity => {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FenixFailureId).HasMaxLength(180).IsRequired();
            entity.HasIndex(x => x.FenixFailureId).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Group)
                  .WithMany(g => g.FailureDefinitions)
                  .HasForeignKey(x => x.GroupId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();
        });

        modelBuilder.Entity<FenixFailureGroup>(entity => {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.System)
                  .WithMany(g => g.FailureGroups)
                  .HasForeignKey(x => x.SystemId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();
        });

        modelBuilder.Entity<FenixFailureSystem>(entity => {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ShortName).HasMaxLength(50).IsRequired();
        });

        #endregion
        modelBuilder.Entity<FailurePreset>(entity => {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(600);
        });

        modelBuilder.Entity<FlightSession>(entity => {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Preset).WithMany().HasForeignKey(x => x.PresetId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TriggeredFailure>(entity => {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.FlightSession).WithMany(x => x.TriggeredFailures).HasForeignKey(x => x.FlightSessionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.FailureDefinition).WithMany().HasForeignKey(x => x.FailureDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Preset).WithMany().HasForeignKey(x => x.PresetId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FailurePreset>()
            .HasMany(x => x.FailureDefinitions)
            .WithMany(x => x.Presets)
            .UsingEntity<Dictionary<string, object>>(
                "PresetFailureDefinition",
                r => r.HasOne<FenixFailureDefinition>().WithMany().HasForeignKey("FailureDefinitionId"),
                l => l.HasOne<FailurePreset>().WithMany().HasForeignKey("FailurePresetId"));

        SeedData.Apply(modelBuilder);
    }
}
