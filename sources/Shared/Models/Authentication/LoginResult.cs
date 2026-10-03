using Shared.Enums;

namespace Shared.Models.Authentication;

/// <summary>Outcome of a login; <see cref="Session"/> is set only for <see cref="LoginStatus.Success"/>.</summary>
public sealed record LoginResult(LoginStatus Status, AuthSession? Session = null);
