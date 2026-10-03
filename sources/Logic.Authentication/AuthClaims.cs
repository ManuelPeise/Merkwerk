namespace Logic.Authentication;

/// <summary>Claim types in the access token (JWT, inbound claim mapping is off).</summary>
public static class AuthClaims
{
    public const string Subject = "sub";

    public const string Name = "name";

    public const string Role = "role";

    /// <summary>"true" while the user still has to replace a start password (LP-104).</summary>
    public const string MustChangePassword = "must_change_password";
}
