using Logic.Authentication;

namespace Logic.Devices.Sessions;

/// <summary>
/// Children's sessions on paired devices (LP-106): sign-in without password, refresh and sign-out. A session ends
/// 8 hours after the sign-in, when the device is unpaired or when the child is deleted. Transport (cookies) is the
/// caller's job.
/// </summary>
public interface ILearnerSessionService
{
    /// <summary>Signs the child in on the device of the token; the child must belong to the device's family.</summary>
    Task<LearnerSignInResult> SignInAsync(string deviceToken, long learnerId, CancellationToken cancellationToken);

    /// <summary>
    /// New access token for the session of the refresh token; <c>null</c> if it is unknown, ended or its device was
    /// unpaired. The refresh token itself stays the same until the session ends (no rotation, see LP-106).
    /// </summary>
    Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Ends the session ("switch child"). Unknown tokens are ignored.</summary>
    Task SignOutAsync(string refreshToken, CancellationToken cancellationToken);
}
