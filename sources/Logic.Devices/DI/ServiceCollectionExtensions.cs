using Logic.Devices.Sessions;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Devices.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Device pairing, children's sessions and the combined refresh/logout path (LP-106, LP-164). Needs Data.Accessor,
    /// Logic.Authentication (TokenService, IAuthSessionService) and Logic.Notifications (IPublicLinkBuilder).
    /// </summary>
    public static IServiceCollection AddMerkwerkDevices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<DeviceService>();
        services.AddScoped<IDeviceService>(sp => sp.GetRequiredService<DeviceService>());
        services.AddScoped<ILearnerSessionService, LearnerSessionService>();
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
