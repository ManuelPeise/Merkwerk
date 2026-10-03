namespace Logic.Authentication.Accounts;

/// <summary>How long links and start passwords stay valid (LP-104).</summary>
public static class AccountTokenLifetimes
{
    /// <summary>Lifetime of Identity's data-protection tokens (password reset, e-mail confirmation).</summary>
    public static readonly TimeSpan LinkToken = TimeSpan.FromHours(2);

    public static readonly TimeSpan StartPassword = TimeSpan.FromHours(24);
}
