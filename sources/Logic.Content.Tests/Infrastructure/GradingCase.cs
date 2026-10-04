using System.Text.Json;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Tests.Infrastructure;

/// <summary>One file of shared/grading-cases (LP-111): question, response and the expected result.</summary>
public sealed record GradingCase(string Description, QuestionContent Question, QuestionResponse Response, GradeResult Expected)
{
    public static GradingCase Load(string path) =>
        JsonSerializer.Deserialize<GradingCase>(File.ReadAllText(path), ExerciseJson.Options)
        ?? throw new InvalidDataException($"{path} is empty.");
}
