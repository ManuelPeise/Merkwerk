using Logic.Content.DI;
using Logic.Content.Grading;
using Logic.Content.Tests.Infrastructure;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Grading;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Tests;

/// <summary>
/// LP-111: every JSON case in shared/grading-cases is graded as expected. The cases are the contract – a client-side
/// grader (offline mode, ADR 001) must pass the same files. One folder per question type.
/// </summary>
public sealed class GradingCaseTests
{
    private static readonly string CasesDirectory = Path.Combine(AppContext.BaseDirectory, "GradingCases");

    /// <summary>Flashcards have only two outcomes (knew it or not), so fewer cases make sense there.</summary>
    private static readonly Dictionary<string, int> MinimumCases = new()
    {
        [QuestionTypes.Choice] = 10,
        [QuestionTypes.Text] = 10,
        [QuestionTypes.Cloze] = 10,
        [QuestionTypes.Match] = 10,
        [QuestionTypes.Flashcard] = 3,
    };

    private readonly GradingService _grading = (GradingService)new ServiceCollection()
        .AddMerkwerkContent()
        .BuildServiceProvider()
        .GetRequiredService<IGradingService>();

    public static TheoryData<string> CaseFiles
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var file in Directory.EnumerateFiles(CasesDirectory, "*.json", SearchOption.AllDirectories).Order())
            {
                data.Add(Path.GetRelativePath(CasesDirectory, file));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(CaseFiles))]
    public void Grade_SharedCase_GivesExpectedResult(string file)
    {
        // Arrange
        var gradingCase = GradingCase.Load(Path.Combine(CasesDirectory, file));

        // Act
        var result = _grading.Grade(gradingCase.Question, gradingCase.Response);

        // Assert
        Assert.Equal(Path.GetDirectoryName(file), TypeOf(gradingCase.Question));
        Assert.Equal(Describe(gradingCase.Expected), Describe(result));
    }

    [Fact]
    public void GradingCases_EveryQuestionType_HasEnoughCases()
    {
        var counts = QuestionTypes.All.ToDictionary(
            type => type,
            type => Directory.Exists(Path.Combine(CasesDirectory, type))
                ? Directory.GetFiles(Path.Combine(CasesDirectory, type), "*.json").Length
                : 0);

        Assert.All(QuestionTypes.All, type => Assert.True(
            counts[type] >= MinimumCases[type], $"{type}: {counts[type]} cases, at least {MinimumCases[type]} expected."));
    }

    [Fact]
    public void Registry_EveryQuestionType_HasExactlyOneGrader()
    {
        Assert.Equal(QuestionTypes.All.Order(), _grading.Types.Order());
    }

    [Fact]
    public void Grade_PayloadAndSolutionOfDifferentTypes_Throws()
    {
        var question = new QuestionContent(
            new ChoicePayload { Prompt = "Was heißt dog?", Options = ["Hund", "Katze"] },
            new TextSolution { AcceptedAnswers = ["Hund"] });

        Assert.Throws<ArgumentException>(() => _grading.Grade(question, new ChoiceResponse { SelectedIndexes = [0] }));
    }

    private static string TypeOf(QuestionContent question) =>
        question.Payload.GetType().Name.Replace("Payload", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

    private static string Describe(GradeResult result) =>
        $"correct={result.IsCorrect}, points={result.Points}/{result.MaxPoints}, hint={result.Hint}, "
        + $"parts=[{string.Join(", ", result.Parts)}]";
}
