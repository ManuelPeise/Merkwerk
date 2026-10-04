using System.Text.Json;
using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Questions;

namespace Logic.Content.Tests;

/// <summary>ADR 005 / LP-110: question content survives a round trip, also with the keys in another order (MySQL).</summary>
public sealed class ExerciseJsonTests
{
    private static readonly QuestionContent[] Questions =
    [
        new QuestionContent(
            new ChoicePayload { Prompt = "Was heißt dog?", Options = ["Hund", "Katze", "Maus"] },
            new ChoiceSolution { CorrectIndexes = [0] }),
        new QuestionContent(
            new TextPayload { Prompt = "7 + 5 =", InputKind = TextInputKind.Number },
            new TextSolution { AcceptedAnswers = ["12"], NumberTolerance = 0 }),
        new QuestionContent(
            new ClozePayload { Prompt = "Setze ein.", Parts = ["Der ", " ist rot."] },
            new ClozeSolution { Gaps = [["Apfel", "Ball"]] }),
        new QuestionContent(
            new MatchPayload { Prompt = "Ordne zu.", Left = ["dog", "cat"], Right = ["Hund", "Katze"] },
            new MatchSolution { RightIndexForLeft = [0, 1] }),
        new QuestionContent(
            new FlashcardPayload { Prompt = "Weißt du es?", Front = "house" },
            new FlashcardSolution { Back = "Haus" }),
    ];

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void RoundTrip_EveryQuestionType_KeepsTypeAndContent(int index)
    {
        var question = Questions[index];

        var json = JsonSerializer.Serialize(question, ExerciseJson.Options);

        var back = JsonSerializer.Deserialize<QuestionContent>(json, ExerciseJson.Options)!;

        Assert.Equal(question.Payload.GetType(), back.Payload.GetType());
        Assert.Equal(question.Solution.GetType(), back.Solution.GetType());
        Assert.Equal(json, JsonSerializer.Serialize(back, ExerciseJson.Options));
    }

    [Fact]
    public void Deserialize_DiscriminatorNotFirst_Works()
    {
        // MySQL returns JSON keys sorted, so "type" can come after the content.
        const string json = """{"options":["Hund","Katze"],"prompt":"Was heißt dog?","multipleAnswers":false,"type":"choice"}""";

        var payload = JsonSerializer.Deserialize<QuestionPayload>(json, ExerciseJson.Options);

        var choice = Assert.IsType<ChoicePayload>(payload);
        Assert.Equal(new[] { "Hund", "Katze" }, choice.Options);
    }

    [Fact]
    public void Serialize_Payload_WritesTypeDiscriminatorAndCamelCase()
    {
        var json = JsonSerializer.Serialize<QuestionPayload>(
            new TextPayload { Prompt = "7 + 5 =", InputKind = TextInputKind.Number }, ExerciseJson.Options);

        Assert.Contains("\"type\":\"text\"", json, StringComparison.Ordinal);
        Assert.Contains("\"inputKind\":\"number\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void QuestionTypes_EveryTypeHasPayloadAndSolution()
    {
        var payloadTypes = typeof(QuestionPayload).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(QuestionPayload))).Select(t => t.Name.Replace("Payload", string.Empty, StringComparison.Ordinal));
        var solutionTypes = typeof(QuestionSolution).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(QuestionSolution))).Select(t => t.Name.Replace("Solution", string.Empty, StringComparison.Ordinal));

        Assert.Equal(payloadTypes.Order(), solutionTypes.Order());
        Assert.Equal(QuestionTypes.All.Count, payloadTypes.Count());
    }
}
