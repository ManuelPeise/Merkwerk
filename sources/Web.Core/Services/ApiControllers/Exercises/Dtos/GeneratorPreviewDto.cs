namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>Generated tasks with the seed that reproduces them (LP-131).</summary>
public sealed record GeneratorPreviewDto(int Seed, IReadOnlyList<QuestionDto> Questions);
