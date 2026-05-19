using Microsoft.EntityFrameworkCore;
using RealFenixFailures.Domain.Entities;

namespace RealFenixFailures.Infrastructure.Persistence;

public class RealFenixDbContext : DbContext
{
    public RealFenixDbContext(DbContextOptions<RealFenixDbContext> options) : base(options)
    {
    }

    public DbSet<FailurePreset> FailurePresets => Set<FailurePreset>();
    public DbSet<FailureDefinition> FailureDefinitions => Set<FailureDefinition>();
    public DbSet<FlightSession> FlightSessions => Set<FlightSession>();
    public DbSet<TriggeredFailure> TriggeredFailures => Set<TriggeredFailure>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FailurePreset>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(600);
        });

        modelBuilder.Entity<FailureDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AffectedSystem).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ExternalFailureId).HasMaxLength(180).IsRequired();
        });

        modelBuilder.Entity<FlightSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Preset).WithMany().HasForeignKey(x => x.PresetId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TriggeredFailure>(entity =>
        {
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
                r => r.HasOne<FailureDefinition>().WithMany().HasForeignKey("FailureDefinitionId"),
                l => l.HasOne<FailurePreset>().WithMany().HasForeignKey("FailurePresetId"));

        SeedData.Apply(modelBuilder);
    }
}
