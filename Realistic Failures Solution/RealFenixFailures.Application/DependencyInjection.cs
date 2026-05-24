using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Application.Services;

namespace RealFenixFailures.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IPresetService, PresetService>();
        services.AddScoped<IInitializerService, InitializerService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IEngineOrchestrator, EngineOrchestrator>();
        services.AddScoped<IInitializerService, InitializerService>();
        return services;
    }
}
