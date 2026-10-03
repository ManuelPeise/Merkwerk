using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.Shared.DI;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddMerkwerkLogicSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}

