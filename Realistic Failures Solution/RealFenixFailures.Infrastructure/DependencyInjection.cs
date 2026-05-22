using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Domain.Interfaces.Repositories;
using RealFenixFailures.Infrastructure.Persistence;
using RealFenixFailures.Infrastructure.Repositories;

namespace RealFenixFailures.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("Sqlite") ?? "Data Source=realfenixfailures.db";

        services.AddDbContext<RealFenixDbContext>(options => options.UseSqlite(connectionString));
        services.Configure<FenixEngineSettings>(configuration.GetSection(FenixEngineSettings.SectionName));
        services.AddScoped<IFailureEngineSettings, FailureEngineSettingsProvider>();

        services.AddScoped<IFenixFailureDefinitionRepository, FenixFailureDefinitionRepository>();
        services.AddScoped<IFailurePresetRepository, FailurePresetRepository>();
        services.AddScoped<IFlightSessionRepository, FlightSessionRepository>();
        services.AddScoped<ITriggeredFailureRepository, TriggeredFailureRepository>();

        return services;
    }
}
