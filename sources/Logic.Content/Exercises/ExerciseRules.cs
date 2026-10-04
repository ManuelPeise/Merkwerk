using Data.Database.Entities.Exercises;

namespace Logic.Content.Exercises;

/// <summary>Limits for exercises and their questions (LP-110).</summary>
public static class ExerciseRules
{
    public const int TitleMaxLength = ExerciseEntity.TitleMaxLength;
    public const int MinGrade = 1;
    public const int MaxGrade = 4;
    public const int MaxQuestions = 50;
    public const int MaxTextLength = 500;
    public const int MinOptions = 2;
    public const int MaxOptions = 8;
    public const int MinPairs = 2;
    public const int MaxPairs = 8;
    public const int MaxGaps = 10;
}
