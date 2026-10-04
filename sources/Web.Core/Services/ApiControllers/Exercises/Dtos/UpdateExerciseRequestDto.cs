using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

public sealed record UpdateExerciseRequestDto([Range(1, long.MaxValue)] long Id, [Required] SaveExerciseRequestDto Exercise);
