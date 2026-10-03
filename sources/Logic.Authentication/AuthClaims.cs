namespace Logic.Authentication;

/// <summary>Claim types in the access token (JWT, inbound claim mapping is off).</summary>
public static class AuthClaims
{
    /// <summary>User id for adults, learner id for children (role <see cref="AuthRoles.Learner"/>).</summary>
    public const string Subject = "sub";

    public const string Name = "name";

    public const string Role = "role";

    /// <summary>Organization the session works in (LP-105); read by HttpCurrentUser for the tenant filter.</summary>
    public const string OrganizationId = "org_id";

    /// <summary>"true" while the user still has to replace a start password (LP-104).</summary>
    public const string MustChangePassword = "must_change_password";

    /// <summary>Child of a learner session (LP-106).</summary>
    public const string LearnerId = "learner_id";

    /// <summary>Paired device a learner session is bound to (LP-106).</summary>
    public const string DeviceId = "device_id";

    /// <summary>Built-in avatar of the child, so the client can restore the header after a reload (LP-106).</summary>
    public const string AvatarId = "avatar_id";
}
