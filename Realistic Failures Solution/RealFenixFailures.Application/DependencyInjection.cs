using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Application.Services;

namespace RealFenixFailures.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IFailurePersistenceService, FailurePersistenceService>();
        services.AddScoped<ITrainingPresetService, TrainingPresetService>();
        services.AddScoped<IPresetService, PresetService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IFailureOrchestrator, FailureOrchestrator>();
        return services;
    }
}
