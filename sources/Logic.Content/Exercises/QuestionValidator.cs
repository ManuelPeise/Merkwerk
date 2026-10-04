using System.Globalization;
using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Questions;

namespace Logic.Content.Exercises;

/// <summary>
/// Checks one question: payload and solution are of the same type and fit together (LP-110). Returns the problems in
/// plain words; the caller prefixes them with the question number.
/// </summary>
internal static class QuestionValidator
{
    public static IReadOnlyList<string> Validate(QuestionContent? question)
    {
        if (question?.Payload is null || question.Solution is null)
        {
            return ["Payload and solution are required."];
        }

        var problems = new List<string>();

        if (!IsFilled(question.Payload.Prompt))
        {
            problems.Add($"Prompt is required, at most {ExerciseRules.MaxTextLength} characters.");
        }

        switch (question.Payload, question.Solution)
        {
            case (ChoicePayload payload, ChoiceSolution solution):
                ValidateChoice(payload, solution, problems);
                break;
            case (TextPayload payload, TextSolution solution):
                ValidateText(payload, solution, problems);
                break;
            case (ClozePayload payload, ClozeSolution solution):
                ValidateCloze(payload, solution, problems);
                break;
            case (MatchPayload payload, MatchSolution solution):
                ValidateMatch(payload, solution, problems);
                break;
            case (FlashcardPayload payload, FlashcardSolution solution):
                if (!IsFilled(payload.Front) || !IsFilled(solution.Back))
                {
                    problems.Add("Front and back are required.");
                }

                break;
            default:
                problems.Add("Payload and solution must be of the same question type.");
                break;
        }

        return problems;
    }

    private static void ValidateChoice(ChoicePayload payload, ChoiceSolution solution, List<string> problems)
    {
        var options = payload.Options ?? [];
        var correct = solution.CorrectIndexes ?? [];

        if (options.Count is < ExerciseRules.MinOptions or > ExerciseRules.MaxOptions || !options.All(IsFilled))
        {
            problems.Add($"Between {ExerciseRules.MinOptions} and {ExerciseRules.MaxOptions} filled options.");
        }

        if (correct.Count == 0 || correct.Distinct().Count() != correct.Count || correct.Any(i => i < 0 || i >= options.Count))
        {
            problems.Add("Correct answers must point to existing options.");
        }
        else if (!payload.MultipleAnswers && correct.Count != 1)
        {
            problems.Add("Exactly one correct answer unless several answers are allowed.");
        }
    }

    private static void ValidateText(TextPayload payload, TextSolution solution, List<string> problems)
    {
        var answers = solution.AcceptedAnswers ?? [];

        if (answers.Count == 0 || !answers.All(IsFilled))
        {
            problems.Add("At least one accepted answer.");
        }
        else if (payload.InputKind == TextInputKind.Number
            && !answers.All(a => decimal.TryParse(a, NumberStyles.Number, CultureInfo.InvariantCulture, out _)))
        {
            problems.Add("Accepted answers of a number question must be numbers (e.g. 12 or 0.5).");
        }

        if (solution.NumberTolerance is < 0)
        {
            problems.Add("Number tolerance must not be negative.");
        }
    }

    private static void ValidateCloze(ClozePayload payload, ClozeSolution solution, List<string> problems)
    {
        var gapCount = (payload.Parts?.Count ?? 0) - 1;
        var gaps = solution.Gaps ?? [];

        if (gapCount is < 1 or > ExerciseRules.MaxGaps)
        {
            problems.Add($"Between 1 and {ExerciseRules.MaxGaps} gaps.");
        }
        else if (gaps.Count != gapCount || gaps.Any(g => g is null || g.Count == 0 || !g.All(IsFilled)))
        {
            problems.Add("Every gap needs at least one accepted answer.");
        }
    }

    private static void ValidateMatch(MatchPayload payload, MatchSolution solution, List<string> problems)
    {
        var left = payload.Left ?? [];
        var right = payload.Right ?? [];
        var mapping = solution.RightIndexForLeft ?? [];

        if (left.Count is < ExerciseRules.MinPairs or > ExerciseRules.MaxPairs || right.Count != left.Count
            || !left.All(IsFilled) || !right.All(IsFilled))
        {
            problems.Add($"Between {ExerciseRules.MinPairs} and {ExerciseRules.MaxPairs} filled pairs.");
        }
        else if (mapping.Count != left.Count || mapping.Order().Select((value, index) => value == index).Contains(false))
        {
            problems.Add("Every left entry needs exactly one right partner.");
        }
    }

    private static bool IsFilled(string? text) =>
        !string.IsNullOrWhiteSpace(text) && text.Length <= ExerciseRules.MaxTextLength;
}
