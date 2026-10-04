using Logic.Content.Subjects;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.Content.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>Subjects (LP-109), later exercises. Needs Data.Accessor and an <see cref="IMemberService"/> (Logic.Organizations).</summary>
    public static IServiceCollection AddMerkwerkContent(this IServiceCollection services)
    {
        services.AddScoped<ISubjectService, SubjectService>();

        return services;
    }
}
