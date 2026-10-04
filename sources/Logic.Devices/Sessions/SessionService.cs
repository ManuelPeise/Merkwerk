using Logic.Shared.Interfaces;
using Shared.Models.Authentication;

namespace Logic.Devices.Sessions;

/// <summary>
/// <see cref="ISessionService"/>: combines the adults' sessions (Logic.Authentication) with the children's sessions
/// (<see cref="LearnerSessionService"/>). Lives in Logic.Devices because only this project knows both.
/// </summary>
internal sealed class SessionService : ISessionService
{
    private readonly IAuthSessionService _authSessionService;
    private readonly ILearnerSessionService _learnerSessionService;

    public SessionService(IAuthSessionService authSessionService, ILearnerSessionService learnerSessionService)
    {
        _authSessionService = authSessionService;
        _learnerSessionService = learnerSessionService;
    }

    public async Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
        await _authSessionService.RefreshAsync(refreshToken, cancellationToken)
        ?? await _learnerSessionService.RefreshAsync(refreshToken, cancellationToken);

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        // Adult chain or child session ("switch child") – each service ignores tokens it doesn't know.
        await _authSessionService.LogoutAsync(refreshToken, cancellationToken);
        await _learnerSessionService.SignOutAsync(refreshToken, cancellationToken);
    }
}
