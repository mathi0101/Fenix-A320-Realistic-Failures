using Microsoft.Extensions.DependencyInjection;
using RealFenixFailures.Domain.Interfaces;
using RealFenixFailures.Domain.Services;

namespace RealFenixFailures.Domain;

public static class DependencyInjection {
    public static IServiceCollection AddDomain(this IServiceCollection services) {
        services.AddScoped<IFailureRuleEvaluator, FailureRuleEvaluator>();
        services.AddScoped<IFailureTrigger, FailureTrigger>();
        services.AddScoped<IFlightHistoryService, FlightHistoryService>();
        return services;
    }
}
