using Logic.Organizations.Groups;
using Logic.Organizations.Invitations;
using Logic.Organizations.Learners;
using Logic.Organizations.Members;
using Logic.Organizations.Setup;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Organizations.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>Setup, invitations, members, child profiles (LP-105) and groups (LP-108). Needs Data.Accessor, Logic.Authentication and Logic.Notifications.</summary>
    public static IServiceCollection AddMerkwerkOrganizations(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<SetupLock>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<ILearnerService, LearnerService>();
        services.AddScoped<IGroupService, GroupService>();

        return services;
    }
}
