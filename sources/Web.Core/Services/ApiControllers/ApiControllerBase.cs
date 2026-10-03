using Microsoft.AspNetCore.Mvc;

namespace Web.Core.Services.ApiControllers;

/// <summary>
/// Base for all API controllers: route <c>/api/v1/{controller}/{action}</c> (lowercase, see ServiceRegistrationExtensions),
/// automatic model validation and ProblemDetails for 4xx results.
/// </summary>
[ApiController]
[Route("api/v1/[controller]/[action]")]
public abstract class ApiControllerBase : ControllerBase
{
}
