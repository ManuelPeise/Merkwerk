namespace Logic.Authentication;

/// <summary>Role names in tokens (mirrored in Web.Client/src/lib/auth/roles.ts). Roles hang on memberships from LP-105/LP-107.</summary>
public static class AuthRoles
{
    public const string Member = "Member";

    public const string OrgAdmin = "OrgAdmin";

    public const string Learner = "Learner";
}
