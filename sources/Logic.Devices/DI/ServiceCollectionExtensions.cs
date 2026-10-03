using Logic.Devices.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Devices.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Device pairing and children's sessions (LP-106). Needs Data.Accessor, Logic.Authentication (TokenService) and
    /// Logic.Notifications (IPublicLinkBuilder).
    /// </summary>
    public static IServiceCollection AddMerkwerkDevices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<DeviceService>();
        services.AddScoped<IDeviceService>(sp => sp.GetRequiredService<DeviceService>());
        services.AddScoped<ILearnerSessionService, LearnerSessionService>();

        return services;
    }
}
