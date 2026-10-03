using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Learners;

namespace Web.Core.Services.ApiControllers.Learners.Dtos;

public sealed record DeleteLearnerRequestDto([Range(1, long.MaxValue)] long Id);
