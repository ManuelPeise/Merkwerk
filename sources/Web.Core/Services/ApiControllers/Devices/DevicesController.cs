using Logic.Devices;
using Logic.Devices.Pairing;
using Logic.Devices.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Web.Core.Bundels;
using Web.Core.Services.ApiControllers.Authentication.Dtos;
using Web.Core.Services.ApiControllers.Devices.Dtos;
using Web.Core.Services.Authorization;
using Web.Core.Services.Cookies;

namespace Web.Core.Services.ApiControllers.Devices;

/// <summary>
/// Paired devices and children's sign-in (LP-106, ADR 006). Adults create pairing codes and manage devices; a device
/// identifies itself only by the HttpOnly cookie <c>mw_device</c> (path <c>/api/v1/devices</c>), so its endpoints are
/// anonymous for the authentication middleware and check the cookie in the service. An unpaired device gets 403 –
/// not 401, which would make the client try a token refresh.
/// </summary>
public sealed class DevicesController(
    IDeviceService deviceService,
    ILearnerSessionService learnerSessionService,
    AuthCookieWriter cookieWriter) : ApiControllerBase
{
    /// <summary>POST /api/v1/devices/pairing-code – six digits, 10 minutes, single use; replaces the family's open code.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType<PairingCodeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PairingCodeDto>> PairingCodeAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await deviceService.CreatePairingCodeAsync(organizationId, userId, cancellationToken) is not { } code)
        {
            return Forbid();
        }

        return PairingCodeDto.From(code);
    }

    /// <summary>POST /api/v1/devices/pair – the tablet sends the code and receives the device cookie (180 days).</summary>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingRegistrationExtensions.DevicePairingPolicy)]
    [HttpPost]
    [ProducesResponseType<PairDeviceResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PairDeviceResponseDto>> PairAsync(
        PairDeviceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await deviceService.PairAsync(request.Code, request.DeviceName ?? string.Empty, cancellationToken);

        if (result.Status != PairStatus.Success)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid pairing code");
        }

        cookieWriter.WriteDevice(Response, result.DeviceToken!, result.DeviceTokenExpiresAt!.Value);
        return new PairDeviceResponseDto(result.FamilyName!);
    }

    /// <summary>GET /api/v1/devices/status – is this device paired? Deletes a cookie that is no longer valid.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<DeviceStatusDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DeviceStatusDto>> StatusAsync(CancellationToken cancellationToken)
    {
        var deviceToken = Request.Cookies[AuthCookies.DeviceToken];

        if (string.IsNullOrEmpty(deviceToken))
        {
            return new DeviceStatusDto(IsPaired: false);
        }

        var status = await deviceService.GetStatusAsync(deviceToken, cancellationToken);

        if (status is null)
        {
            cookieWriter.DeleteDevice(Response);
            return new DeviceStatusDto(IsPaired: false);
        }

        return new DeviceStatusDto(IsPaired: true, status.FamilyName);
    }

    /// <summary>GET /api/v1/devices/profiles – the children of the device's family (first name and avatar).</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DeviceProfileDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<DeviceProfileDto>>> ProfilesAsync(CancellationToken cancellationToken)
    {
        var profiles = await deviceService.ListProfilesAsync(Request.Cookies[AuthCookies.DeviceToken] ?? string.Empty, cancellationToken);

        if (profiles is null)
        {
            return DeviceNotPaired();
        }

        return profiles.Select(DeviceProfileDto.From).ToList();
    }

    /// <summary>
    /// POST /api/v1/devices/sign-in – signs the child in (role Learner, 8 hours) and sets the auth cookies; renews the
    /// device cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> SignInAsync(DeviceSignInRequestDto request, CancellationToken cancellationToken)
    {
        var deviceToken = Request.Cookies[AuthCookies.DeviceToken] ?? string.Empty;
        var result = await learnerSessionService.SignInAsync(deviceToken, request.LearnerId, cancellationToken);

        switch (result.Status)
        {
            case LearnerSignInStatus.Success:
                cookieWriter.Write(Response, result.Session!);
                cookieWriter.WriteDevice(Response, deviceToken, result.DeviceTokenExpiresAt!.Value);
                return SessionDto.From(result.Session!);
            case LearnerSignInStatus.DeviceNotPaired:
                return DeviceNotPaired();
            default:
                return NotFound();
        }
    }

    /// <summary>GET /api/v1/devices/list – paired devices of the family.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DeviceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<DeviceDto>>> ListAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId
            || await deviceService.ListAsync(organizationId, userId, cancellationToken) is not { } devices)
        {
            return Forbid();
        }

        return devices.Select(DeviceDto.From).ToList();
    }

    /// <summary>POST /api/v1/devices/revoke – unpairs the device; its children are signed out at the next refresh.</summary>
    [Authorize(Policy = AuthorizationPolicies.Member)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAsync(RevokeDeviceRequestDto request, CancellationToken cancellationToken)
    {
        if (CurrentOrganizationId is not { } organizationId || CurrentUserId is not { } userId)
        {
            return NotFound();
        }

        return await deviceService.RevokeAsync(organizationId, userId, request.Id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    private ObjectResult DeviceNotPaired()
    {
        cookieWriter.DeleteDevice(Response);
        return Problem(statusCode: StatusCodes.Status403Forbidden, title: "Device not paired");
    }
}
