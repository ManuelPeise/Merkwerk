namespace Logic.Notifications.Links;

/// <summary>Builds absolute links for mails. Links carry tokens only, never personal data (LP-162).</summary>
public interface IPublicLinkBuilder
{
    /// <summary>e.g. <c>Build("/confirm-email", ("token", token))</c> → <c>https://host/confirm-email?token=…</c>.</summary>
    string Build(string path, params (string Name, string Value)[] query);
}
