using Logic.Content.Exercises;
using Logic.Content.Grading;
using Logic.Content.Subjects;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logic.Content.DI;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Subjects (LP-109), exercises (LP-110) and grading (LP-111). Needs Data.Accessor and an <see cref="IMemberService"/> (Logic.Organizations).
    /// </summary>
    public static IServiceCollection AddMerkwerkContent(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IExerciseService, ExerciseService>();

        // Graders are stateless; one per question type, picked by GradingService.
        services.AddSingleton<IQuestionGrader, ChoiceGrader>();
        services.AddSingleton<IQuestionGrader, TextGrader>();
        services.AddSingleton<IQuestionGrader, ClozeGrader>();
        services.AddSingleton<IQuestionGrader, MatchGrader>();
        services.AddSingleton<IQuestionGrader, FlashcardGrader>();
        services.AddSingleton<IGradingService, GradingService>();

        return services;
    }
}
