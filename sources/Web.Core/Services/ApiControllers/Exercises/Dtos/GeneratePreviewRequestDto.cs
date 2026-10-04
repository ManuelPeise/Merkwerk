using System.ComponentModel.DataAnnotations;
using Shared.Models.Exercises.Generators;

namespace Web.Core.Services.ApiControllers.Exercises.Dtos;

/// <summary>Settings to try out; without a seed the server picks one (LP-131).</summary>
public sealed record GeneratePreviewRequestDto([Required] GeneratorSettings Generator, int? Seed);
