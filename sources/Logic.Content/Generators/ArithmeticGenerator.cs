using System.Globalization;
using Shared.Enums;
using Shared.Models.Exercises;
using Shared.Models.Exercises.Generators;
using Shared.Models.Exercises.Questions;

namespace Logic.Content.Generators;

/// <summary>
/// Basic arithmetic (LP-131). Operands are at least 1, results never negative, division without remainder. Times and
/// divide use one factor up to 10 (number range 100 = the small times table). Ten transition: see
/// <see cref="TenTransition"/>.
/// </summary>
internal sealed class ArithmeticGenerator : IExerciseGenerator
{
    public const int MinNumberRange = 5;
    public const int MaxNumberRange = 1000;
    public const int MinTaskCount = 1;
    public const int MaxTaskCount = 50;

    /// <summary>The ten transition only makes sense from 20 on (up to 10 nothing crosses a ten).</summary>
    public const int MinRangeForTenTransition = 20;

    private const int MaxFactor = 10;
    private const int MaxAttemptsPerTask = 1000;

    /// <summary>Retries to avoid the same task twice in one set (small number ranges may still repeat).</summary>
    private const int MaxRetriesForUniqueTask = 20;

    public Type SettingsType => typeof(ArithmeticSettings);

    public Dictionary<string, string[]> Validate(GeneratorSettings settings)
    {
        var arithmetic = (ArithmeticSettings)settings;
        var errors = new Dictionary<string, string[]>();
        var operations = arithmetic.Operations ?? [];

        if (operations.Count == 0 || !operations.All(Enum.IsDefined))
        {
            errors["generator.operations"] = ["Choose at least one of add, subtract, multiply, divide."];
        }

        if (arithmetic.NumberRange is < MinNumberRange or > MaxNumberRange)
        {
            errors["generator.numberRange"] = [$"Between {MinNumberRange} and {MaxNumberRange}."];
        }

        if (arithmetic.TaskCount is < MinTaskCount or > MaxTaskCount)
        {
            errors["generator.taskCount"] = [$"Between {MinTaskCount} and {MaxTaskCount} tasks."];
        }

        if (!Enum.IsDefined(arithmetic.TenTransition) || !Enum.IsDefined(arithmetic.Placeholder))
        {
            errors["generator"] = ["Unknown ten transition or placeholder mode."];
        }
        else if (arithmetic.TenTransition != TenTransition.Any
            && (arithmetic.NumberRange < MinRangeForTenTransition
                || !operations.Any(o => o is ArithmeticOperation.Add or ArithmeticOperation.Subtract)))
        {
            errors["generator.tenTransition"] =
                [$"Needs addition or subtraction and a number range of at least {MinRangeForTenTransition}."];
        }

        return errors;
    }

    public IReadOnlyList<QuestionContent> Generate(GeneratorSettings settings, int seed) =>
        CreateTasks((ArithmeticSettings)settings, seed).Select(ToQuestion).ToList();

    /// <summary>The tasks behind <see cref="Generate"/> – the tests check the rules on them.</summary>
    internal static IReadOnlyList<ArithmeticTask> CreateTasks(ArithmeticSettings settings, int seed)
    {
        var random = new SeededRandom(seed);
        var tasks = new List<ArithmeticTask>(settings.TaskCount);
        var seen = new HashSet<(int, ArithmeticOperation, int)>();

        for (var index = 0; index < settings.TaskCount; index++)
        {
            ArithmeticTask task;
            var retries = 0;

            do
            {
                task = CreateTask(settings, random);
            }
            while (!seen.Add((task.Left, task.Operation, task.Right)) && ++retries < MaxRetriesForUniqueTask);

            tasks.Add(task);
        }

        return tasks;
    }

    private static ArithmeticTask CreateTask(ArithmeticSettings settings, SeededRandom random)
    {
        var operation = settings.Operations[random.Next(0, settings.Operations.Count - 1)];

        for (var attempt = 0; attempt < MaxAttemptsPerTask; attempt++)
        {
            var (left, right, result) = operation switch
            {
                ArithmeticOperation.Add => Add(settings.NumberRange, random),
                ArithmeticOperation.Subtract => Subtract(settings.NumberRange, random),
                ArithmeticOperation.Multiply => Multiply(settings.NumberRange, random),
                _ => Divide(settings.NumberRange, random),
            };

            if (FitsTenTransition(settings, operation, left, right))
            {
                return new ArithmeticTask(left, operation, right, result, ChooseMissing(settings.Placeholder, random));
            }
        }

        throw new InvalidOperationException("No task fits the settings – validate them first.");
    }

    private static (int Left, int Right, int Result) Add(int range, SeededRandom random)
    {
        var left = random.Next(1, range - 1);
        var right = random.Next(1, range - left);
        return (left, right, left + right);
    }

    private static (int Left, int Right, int Result) Subtract(int range, SeededRandom random)
    {
        var left = random.Next(2, range);
        var right = random.Next(1, left);
        return (left, right, left - right);
    }

    private static (int Left, int Right, int Result) Multiply(int range, SeededRandom random)
    {
        var factor = random.Next(1, Math.Min(MaxFactor, range));
        var other = random.Next(1, range / factor);

        // The small factor is not always on the same side: 3 · 7 and 7 · 3.
        return random.NextBool() ? (factor, other, factor * other) : (other, factor, factor * other);
    }

    private static (int Left, int Right, int Result) Divide(int range, SeededRandom random)
    {
        var divisor = random.Next(1, Math.Min(MaxFactor, range));
        var quotient = random.Next(1, range / divisor);
        return (divisor * quotient, divisor, quotient);
    }

    private static bool FitsTenTransition(ArithmeticSettings settings, ArithmeticOperation operation, int left, int right)
    {
        if (settings.TenTransition == TenTransition.Any
            || operation is not (ArithmeticOperation.Add or ArithmeticOperation.Subtract))
        {
            return true;
        }

        return CrossesTen(operation, left, right) == (settings.TenTransition == TenTransition.With);
    }

    /// <summary>8 + 5: the ones go past the next ten. 13 − 5: the ones of the first number are not enough.</summary>
    internal static bool CrossesTen(ArithmeticOperation operation, int left, int right) =>
        operation == ArithmeticOperation.Add ? (left % 10) + (right % 10) > 10 : left % 10 < right % 10;

    private static ArithmeticPart ChooseMissing(PlaceholderMode placeholder, SeededRandom random)
    {
        var usePlaceholder = placeholder == PlaceholderMode.Only
            || (placeholder == PlaceholderMode.Mixed && random.NextBool());

        if (!usePlaceholder)
        {
            return ArithmeticPart.Result;
        }

        return random.NextBool() ? ArithmeticPart.Left : ArithmeticPart.Right;
    }

    private static QuestionContent ToQuestion(ArithmeticTask task)
    {
        const string Box = "□";
        var symbol = task.Operation switch
        {
            ArithmeticOperation.Add => "+",
            ArithmeticOperation.Subtract => "−",
            ArithmeticOperation.Multiply => "·",
            _ => ":",
        };

        var (prompt, answer) = task.Missing switch
        {
            ArithmeticPart.Left => ($"{Box} {symbol} {Text(task.Right)} = {Text(task.Result)}", task.Left),
            ArithmeticPart.Right => ($"{Text(task.Left)} {symbol} {Box} = {Text(task.Result)}", task.Right),
            _ => ($"{Text(task.Left)} {symbol} {Text(task.Right)} =", task.Result),
        };

        return new QuestionContent(
            new TextPayload { Prompt = prompt, InputKind = TextInputKind.Number },
            new TextSolution { AcceptedAnswers = [Text(answer)] });
    }

    private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);
}
