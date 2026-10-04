using Shared.Enums;

namespace Logic.Content.Generators;

/// <summary>
/// One generated task: <c>Left op Right = Result</c>. <see cref="Missing"/> says which number the child fills in
/// (the result, or an operand for placeholder tasks).
/// </summary>
internal sealed record ArithmeticTask(int Left, ArithmeticOperation Operation, int Right, int Result, ArithmeticPart Missing);
