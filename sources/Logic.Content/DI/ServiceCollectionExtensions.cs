using Logic.Content.Exercises;
using Logic.Content.Subjects;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Content.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Subjects (LP-109) and exercises (LP-110). Needs Data.Accessor and an <see cref="IMemberService"/> (Logic.Organizations).
    /// </summary>
    public static IServiceCollection AddMerkwerkContent(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IExerciseService, ExerciseService>();

        return services;
    }
}
