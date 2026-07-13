using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealFenixFailures.Application.Interfaces;
using RealFenixFailures.Application.Services;

namespace RealFenixFailures.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddScoped<IPresetService, PresetService>();
        services.AddScoped<IInitializerService, InitializerService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IEngineOrchestrator, EngineOrchestrator>();
        services.AddSingleton<ISimulatorConnectionService, SimulatorConnectionService>();
        services.AddTransient<IRealisticSessionManager>((sp) =>
            new RealisticSessionManager(
                sp.GetRequiredService<ILogger<RealisticSessionManager>>(),
                sp.GetRequiredService<ISimulatorConnectionService>(),
                sp.GetRequiredService<IPresetService>()
            )
        );
        return services;
    }
}