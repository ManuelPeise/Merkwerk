using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>archived = false restores the exercise.</summary>
public sealed record ArchiveExerciseRequestDto([Range(1, long.MaxValue)] long Id, bool Archived);
