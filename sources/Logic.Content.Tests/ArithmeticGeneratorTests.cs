using System.Text.Json;
using Logic.Content.DI;
using Logic.Content.Generators;
using Logic.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;
using Shared.Models.Exercises.Questions;
using Shared.Models.Exercises.Responses;

namespace Logic.Content.Tests;

/// <summary>LP-131: the arithmetic generator keeps its rules for many seeds and is deterministic.</summary>
public sealed class ArithmeticGeneratorTests
{
    private const int SeedsPerSetting = 200;

    private static readonly ArithmeticSettings[] Settings =
    [
        new() { Operations = [ArithmeticOperation.Add], NumberRange = 20 },
        new() { Operations = [ArithmeticOperation.Subtract], NumberRange = 20, TenTransition = TenTransition.Without, Placeholder = PlaceholderMode.Mixed },
        new() { Operations = [ArithmeticOperation.Add, ArithmeticOperation.Subtract], NumberRange = 100, TenTransition = TenTransition.With, Placeholder = PlaceholderMode.Only },
        new() { Operations = [ArithmeticOperation.Multiply], NumberRange = 100 },
        new() { Operations = [ArithmeticOperation.Divide], NumberRange = 100, Placeholder = PlaceholderMode.Mixed },
        new()
        {
            Operations = [ArithmeticOperation.Add, ArithmeticOperation.Subtract, ArithmeticOperation.Multiply, ArithmeticOperation.Divide],
            NumberRange = 1000,
            Placeholder = PlaceholderMode.Mixed,
            TaskCount = 50,
        },
        new() { Operations = [ArithmeticOperation.Add], NumberRange = 10 },
        new() { Operations = [ArithmeticOperation.Subtract], NumberRange = 20, TenTransition = TenTransition.With },
        new() { Operations = [ArithmeticOperation.Multiply, ArithmeticOperation.Divide], NumberRange = 50, TaskCount = 20 },
    ];

    private readonly ServiceProvider _services = new ServiceCollection().AddMerkwerkContent().BuildServiceProvider();

    private IGeneratorService Generators => _services.GetRequiredService<IGeneratorService>();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void CreateTasks_ManySeeds_KeepAllRules(int index)
    {
        var settings = Settings[index];

        for (var seed = 0; seed < SeedsPerSetting; seed++)
        {
            var tasks = ArithmeticGenerator.CreateTasks(settings, seed);

            Assert.Equal(settings.TaskCount, tasks.Count);
            Assert.All(tasks, task => AssertRules(settings, task));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(5)]
    public void Generate_SameSeed_SameTasks_OtherSeed_OtherTasks(int index)
    {
        var settings = Settings[index];

        var first = Json(Generators.Generate(settings, 4711));
        var again = Json(Generators.Generate(settings, 4711));
        var other = Json(Generators.Generate(settings, 4712));

        Assert.Equal(first, again);
        Assert.NotEqual(first, other);
    }

    [Fact]
    public void Generate_Placeholder_ShowsBoxAndExpectsTheMissingNumber()
    {
        var settings = new ArithmeticSettings { NumberRange = 20, Placeholder = PlaceholderMode.Only };

        var questions = Generators.Generate(settings, 1);

        Assert.All(questions, q => Assert.Contains("□", Assert.IsType<TextPayload>(q.Payload).Prompt, StringComparison.Ordinal));
    }

    [Fact]
    public void Generate_Questions_AreNumberQuestionsGradedRightWithTheirAnswer()
    {
        var grading = _services.GetRequiredService<IGradingService>();

        var questions = Generators.Generate(Settings[5], 99);

        Assert.All(questions, question =>
        {
            Assert.Equal(TextInputKind.Number, Assert.IsType<TextPayload>(question.Payload).InputKind);
            var answer = Assert.Single(Assert.IsType<TextSolution>(question.Solution).AcceptedAnswers);
            Assert.True(grading.Grade(question, new TextResponse { Text = answer }).IsCorrect);
            Assert.False(grading.Grade(question, new TextResponse { Text = answer + "1" }).IsCorrect);
        });
    }

    [Fact]
    public void Generate_SmallRange_RepeatsTasksOnlyWhenItMust()
    {
        // 45 different additions exist up to 10 – plenty for 5 tasks without repetition.
        var tasks = ArithmeticGenerator.CreateTasks(new ArithmeticSettings { NumberRange = 10, TaskCount = 5 }, 3);

        Assert.Equal(tasks.Count, tasks.Select(t => (t.Left, t.Right)).Distinct().Count());
    }

    private static readonly ArithmeticSettings[] InvalidSettings =
    [
        new() { Operations = [] },
        new() { NumberRange = 4 },
        new() { NumberRange = 1001 },
        new() { TaskCount = 0 },
        new() { TaskCount = 51 },
        new() { NumberRange = 10, TenTransition = TenTransition.With },
        new() { Operations = [ArithmeticOperation.Multiply], TenTransition = TenTransition.Without },
        new() { Operations = [(ArithmeticOperation)99] },
    ];

    [Theory]
    [InlineData(0, "generator.operations")]
    [InlineData(1, "generator.numberRange")]
    [InlineData(2, "generator.numberRange")]
    [InlineData(3, "generator.taskCount")]
    [InlineData(4, "generator.taskCount")]
    [InlineData(5, "generator.tenTransition")]
    [InlineData(6, "generator.tenTransition")]
    [InlineData(7, "generator.operations")]
    public void Validate_InvalidSettings_ReportsField(int index, string field)
    {
        var errors = Generators.Validate(InvalidSettings[index]);

        Assert.Contains(field, errors.Keys);
        Assert.Throws<ArgumentException>(() => Generators.Generate(InvalidSettings[index], 1));
    }

    [Fact]
    public void Validate_MissingSettings_ReportsGenerator()
    {
        Assert.Contains("generator", Generators.Validate(null).Keys);
    }

    [Fact]
    public void Validate_DefaultSettings_AreValid()
    {
        Assert.Empty(Generators.Validate(new ArithmeticSettings()));
    }

    private static void AssertRules(ArithmeticSettings settings, ArithmeticTask task)
    {
        var range = settings.NumberRange;

        Assert.Contains(task.Operation, settings.Operations);
        Assert.InRange(task.Left, 1, range);
        Assert.InRange(task.Right, 1, range);
        Assert.InRange(task.Result, 0, range);

        switch (task.Operation)
        {
            case ArithmeticOperation.Add:
                Assert.Equal(task.Left + task.Right, task.Result);
                break;
            case ArithmeticOperation.Subtract:
                Assert.Equal(task.Left - task.Right, task.Result);
                break;
            case ArithmeticOperation.Multiply:
                Assert.Equal(task.Left * task.Right, task.Result);
                Assert.True(Math.Min(task.Left, task.Right) <= 10, $"{task}: no factor up to 10");
                break;
            case ArithmeticOperation.Divide:
                Assert.Equal(task.Left, task.Right * task.Result);
                Assert.InRange(task.Right, 1, 10);
                break;
        }

        if (settings.TenTransition != TenTransition.Any
            && task.Operation is ArithmeticOperation.Add or ArithmeticOperation.Subtract)
        {
            Assert.Equal(
                settings.TenTransition == TenTransition.With,
                ArithmeticGenerator.CrossesTen(task.Operation, task.Left, task.Right));
        }

        switch (settings.Placeholder)
        {
            case PlaceholderMode.None:
                Assert.Equal(ArithmeticPart.Result, task.Missing);
                break;
            case PlaceholderMode.Only:
                Assert.NotEqual(ArithmeticPart.Result, task.Missing);
                break;
        }
    }

    private static string Json(IReadOnlyList<QuestionContent> questions) =>
        JsonSerializer.Serialize(questions, ExerciseJson.Options);
}
