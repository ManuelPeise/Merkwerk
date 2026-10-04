using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

public sealed record ExerciseIdRequestDto([Range(1, long.MaxValue)] long Id);
