namespace Logic.Authentication.Accounts;

/// <summary>Outcome of an account operation. <see cref="Errors"/> are Identity's messages (e.g. password rules).</summary>
public sealed record AccountResult(bool Succeeded, IReadOnlyList<string> Errors, AuthSession? Session = null)
{
    public static AccountResult Success(AuthSession? session = null) => new(true, [], session);

    public static AccountResult Failed(params string[] errors) => new(false, errors);
}
